using FitLife.Community.Api.Models;

namespace FitLife.Community.Api.DTOs;

public record CommunityActivityDto(
    Guid PostId,
    Guid CommunityId,
    Center Center,
    string CommunityName,
    Guid MemberId,
    string AuthorName,
    string Content,
    DateTime CreatedAt);
