using System.Text;
using System.Text.Json;
using FitLife.Community.Api.IntegrationEvents;
using FitLife.Community.Api.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FitLife.Community.Api.Messaging;

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

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

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
                await channel.BasicAckAsync(args.DeliveryTag, multiple: false);

                _logger.LogInformation(
                    "Consumed MemberCreated event for MemberId {MemberId}",
                    memberCreatedEvent.MemberId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not handle MemberCreated event");
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(
            queue: queue,
            autoAck: false,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}