namespace FitLife.Community.Api.IntegrationEvents;

public record MemberCreatedEvent(
    Guid EventId,
    Guid MemberId,
    Guid UserId,
    string FullName,
    string Email,
    string PrimaryCenter,
    string MembershipType,
    DateTime OccurredAtUtc);