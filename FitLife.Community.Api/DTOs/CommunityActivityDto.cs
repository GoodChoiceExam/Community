using FitLife.Community.Api.Models;

namespace FitLife.Community.Api.DTOs;

// Fladt read-only objekt der returneres til frontend med seneste aktivitet på tværs af communities.
public record CommunityActivityDto(
    Guid PostId,
    Guid CommunityId,
    Center Center,
    string CommunityName,
    Guid MemberId,
    string AuthorName,
    string Content,
    DateTime CreatedAt);
