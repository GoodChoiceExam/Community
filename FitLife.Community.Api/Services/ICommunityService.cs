using FitLife.Community.Api.DTOs;
using FitLife.Community.Api.Models;

namespace FitLife.Community.Api.Services;

public interface ICommunityService
{
    Task<List<CenterCommunity>> GetCommunitiesAsync();
    Task<CenterCommunity?> GetCommunityByIdAsync(Guid id);
    Task<CenterCommunity> CreateCommunityAsync(CreateCommunityRequest request);
    Task<List<CommunityPost>?> GetPostsAsync(Guid communityId);
    Task<CommunityPost?> CreatePostAsync(Guid communityId, Guid memberId, string authorName, CreateCommunityPostRequest request);
    Task<List<CommunityActivityDto>> GetRecentActivityAsync(int take = 20);
}
