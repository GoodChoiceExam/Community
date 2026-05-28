namespace FitLife.Community.Api.IntegrationEvents;

// Event der modtages fra Membership-servicen via RabbitMQ når et nyt medlem oprettes.
public record MemberCreatedEvent(
    Guid EventId,
    Guid MemberId,
    Guid UserId,
    string FullName,
    string Email,
    string PrimaryCenter,
    string MembershipType,
    DateTime OccurredAtUtc);