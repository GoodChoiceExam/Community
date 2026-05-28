using FitLife.Community.Api.DTOs;
using FitLife.Community.Api.Models;
using FitLife.Community.Api.IntegrationEvents;

namespace FitLife.Community.Api.Services;

// Definerer kontrakten for community-forretningslogik.
// Implementeres af CommunityService og kan mockes i tests.
public interface ICommunityService
{
    Task<List<CenterCommunity>> GetCommunitiesAsync();
    Task<List<CommunityPost>?> GetPostsAsync(Guid communityId);
    Task<CommunityPost?> CreatePostAsync(Guid communityId, Guid memberId, string authorName, CreateCommunityPostRequest request);
    Task HandleMemberCreatedAsync(MemberCreatedEvent memberCreatedEvent);
    Task<CenterCommunity?> GetCommunityByCenterAsync(Center center);
}
