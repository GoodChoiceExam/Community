using FitLife.Community.Api.Models;

namespace FitLife.Community.Api.Repositories;

// Definerer kontrakten for databaseoperationer på communities og posts.
// Implementeres af CommunityRepository og kan mockes i tests.
public interface ICommunityRepository
{
    Task<List<CenterCommunity>> GetCommunitiesAsync();
    Task<CenterCommunity?> GetCommunityByIdAsync(Guid id);
    Task<CenterCommunity?> GetCommunityByCenterAsync(Center center);
    Task<CenterCommunity> AddCommunityAsync(CenterCommunity community);
    Task<List<CommunityPost>> GetPostsByCommunityAsync(Guid communityId);
    Task<CommunityPost> AddPostAsync(CommunityPost post);
}
