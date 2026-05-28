using System.Text;
using System.Text.Json;
using FitLife.Community.Api.IntegrationEvents;
using FitLife.Community.Api.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace FitLife.Community.Api.Messaging;

// BackgroundService der lytter på RabbitMQ-køen "membership.member.created".
// Når Membership-servicen opretter et nyt medlem, modtager denne service eventet
// og opretter automatisk en community-gruppe for medlemmets center.
public class MemberCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ICommunityService _communityService;
    private readonly ILogger<MemberCreatedConsumer> _logger;

    public MemberCreatedConsumer(
        IConfiguration configuration,
        ICommunityService communityService,
        ILogger<MemberCreatedConsumer> logger)
    {
        _configuration = configuration;
        _communityService = communityService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Forbinder til RabbitMQ
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"]
                       ?? throw new InvalidOperationException("RabbitMQ host is missing"),
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:Username"]
                       ?? throw new InvalidOperationException("RabbitMQ username is missing"),
            Password = _configuration["RabbitMQ:Password"]
                       ?? throw new InvalidOperationException("RabbitMQ password is missing")
        };

        const string queue = "membership.member.created";

        IConnection? connection = null;
        // Prøver at forbinde med eksponentielt stigende forsinkelse for at give RabbitMQ tid til at starte
        int[] retryDelaysSeconds = [2, 4, 8, 16, 30];
        foreach (var delay in retryDelaysSeconds)
        {
            try
            {
                connection = await factory.CreateConnectionAsync(stoppingToken);
                break;
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogWarning(ex, "RabbitMQ not reachable, retrying in {Delay}s", delay);
                await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
            }
        }

        if (connection is null)
            throw new InvalidOperationException("Could not connect to RabbitMQ after retries");

        await using var _ = connection;
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var memberCreatedEvent = JsonSerializer.Deserialize<MemberCreatedEvent>(json);

                if (memberCreatedEvent is null)
                {
                    await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                await _communityService.HandleMemberCreatedAsync(memberCreatedEvent);
                // Bekræfter at beskeden er behandlet — RabbitMQ fjerner den fra køen
                await channel.BasicAckAsync(args.DeliveryTag, multiple: false);

                _logger.LogInformation(
                    "Consumed MemberCreated event for MemberId {MemberId}",
                    memberCreatedEvent.MemberId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not handle MemberCreated event");
                // requeue: false — fejlede beskeder smides væk i stedet for at blive sendt igen i en løkke
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(
            queue: queue,
            autoAck: false,
            consumer: consumer);

        // Holder servicen i live indtil applikationen stoppes
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}