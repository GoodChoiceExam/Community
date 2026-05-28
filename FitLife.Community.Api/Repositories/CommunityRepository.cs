using FitLife.Community.Api.Models;
using MongoDB.Driver;

namespace FitLife.Community.Api.Repositories;

// MongoDB-implementering af ICommunityRepository.
// Håndterer to collections: communities og communityPosts.
public class CommunityRepository : ICommunityRepository
{
    private readonly IMongoCollection<CenterCommunity> _communities;
    private readonly IMongoCollection<CommunityPost> _posts;

    public CommunityRepository(IMongoDatabase database)
    {
        _communities = database.GetCollection<CenterCommunity>("communities");
        _posts = database.GetCollection<CommunityPost>("communityPosts");
    }

    public async Task<List<CenterCommunity>> GetCommunitiesAsync()
    {
        // Sorterer efter center-enum så rækkefølgen er konsistent
        return await _communities.Find(_ => true)
            .SortBy(c => c.Center)
            .ToListAsync();
    }

    public async Task<CenterCommunity?> GetCommunityByIdAsync(Guid id)
    {
        return await _communities.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<CenterCommunity?> GetCommunityByCenterAsync(Center center)
    {
        return await _communities.Find(c => c.Center == center).FirstOrDefaultAsync();
    }

    public async Task<CenterCommunity> AddCommunityAsync(CenterCommunity community)
    {
        await _communities.InsertOneAsync(community);
        return community;
    }

    public async Task<List<CommunityPost>> GetPostsByCommunityAsync(Guid communityId)
    {
        // Nyeste posts vises først
        return await _posts.Find(p => p.CommunityId == communityId)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<CommunityPost> AddPostAsync(CommunityPost post)
    {
        await _posts.InsertOneAsync(post);
        return post;
    }
}
