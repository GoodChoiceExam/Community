using FitLife.Community.Api.DTOs;
using FitLife.Community.Api.Models;
using MongoDB.Driver;
using FitLife.Community.Api.IntegrationEvents;

namespace FitLife.Community.Api.Services;

public class CommunityService : ICommunityService
{
    private readonly IMongoCollection<CenterCommunity> _communities;
    private readonly IMongoCollection<CommunityPost> _posts;

    public CommunityService(IConfiguration configuration)
    {
        var mongoConn = configuration["MongoDB:ConnectionString"]!;
        var mongoDb = configuration["MongoDB:DatabaseName"]!;
        var communitiesCollectionName = configuration["MongoDB:CommunitiesCollectionName"] ?? "communities";
        var postsCollectionName = configuration["MongoDB:PostsCollectionName"] ?? "communityPosts";

        var client = new MongoClient(mongoConn);
        var database = client.GetDatabase(mongoDb);
        _communities = database.GetCollection<CenterCommunity>(communitiesCollectionName);
        _posts = database.GetCollection<CommunityPost>(postsCollectionName);
    }

    public async Task<List<CenterCommunity>> GetCommunitiesAsync()
    {
        return await _communities.Find(_ => true)
            .SortBy(community => community.Center)
            .ToListAsync();
    }

    public async Task<CenterCommunity?> GetCommunityByIdAsync(Guid id)
    {
        return await _communities.Find(community => community.Id == id).FirstOrDefaultAsync();
    }

    public async Task<CenterCommunity> CreateCommunityAsync(CreateCommunityRequest request)
    {
        var community = new CenterCommunity
        {
            Center = request.Center!.Value,
            Name = request.Name.Trim()
        };

        await _communities.InsertOneAsync(community);
        return community;
    }

    public async Task<List<CommunityPost>?> GetPostsAsync(Guid communityId)
    {
        var community = await GetCommunityByIdAsync(communityId);
        if (community is null)
            return null;

        return await _posts.Find(post => post.CommunityId == communityId)
            .SortByDescending(post => post.CreatedAt)
            .ToListAsync();
    }

    public async Task<CommunityPost?> CreatePostAsync(
        Guid communityId,
        Guid memberId,
        string authorName,
        CreateCommunityPostRequest request)
    {
        var community = await GetCommunityByIdAsync(communityId);
        if (community is null)
            return null;

        var post = new CommunityPost
        {
            CommunityId = communityId,
            MemberId = memberId,
            AuthorName = authorName.Trim(),
            Content = request.Content.Trim()
        };

        await _posts.InsertOneAsync(post);
        return post;
    }

    public async Task<List<CommunityActivityDto>> GetRecentActivityAsync(int take = 20)
    {
        take = Math.Clamp(take, 1, 100);
        var posts = await _posts.Find(_ => true)
            .SortByDescending(post => post.CreatedAt)
            .Limit(take)
            .ToListAsync();

        if (posts.Count == 0)
            return [];

        var communityIds = posts.Select(post => post.CommunityId).Distinct().ToArray();
        var communities = await _communities.Find(community => communityIds.Contains(community.Id)).ToListAsync();
        var communityById = communities.ToDictionary(community => community.Id);

        return posts
            .Where(post => communityById.ContainsKey(post.CommunityId))
            .Select(post =>
            {
                var community = communityById[post.CommunityId];
                return new CommunityActivityDto(
                    post.Id,
                    community.Id,
                    community.Center,
                    community.Name,
                    post.MemberId,
                    post.AuthorName,
                    post.Content,
                    post.CreatedAt);
            })
            .ToList();
    }
    
    public async Task HandleMemberCreatedAsync(MemberCreatedEvent memberCreatedEvent)
    {
        var center = MapCenter(memberCreatedEvent.PrimaryCenter);

        var existingCommunity = await _communities
            .Find(community => community.Center == center)
            .FirstOrDefaultAsync();

        if (existingCommunity is not null)
            return;

        var community = new CenterCommunity
        {
            Center = center,
            Name = $"{FormatCenterName(center)} Gruppe"
        };

        await _communities.InsertOneAsync(community);
    }

    public async Task<CenterCommunity?> GetCommunityByCenterAsync(Center center)
    {
        return await _communities
            .Find(community => community.Center == center)
            .FirstOrDefaultAsync();
    }

    private static Center MapCenter(string primaryCenter)
    {
        return primaryCenter switch
        {
            "Vesterbro" => Center.Vesterbro,
            "Nørrebro" => Center.Nørrebro,
            "Østerbro" => Center.Østerbro,
            "AarhusC" => Center.AarhusC,
            "Kolding" => Center.Kolding,
            _ => throw new ArgumentException($"Unknown center: {primaryCenter}")
        };
    }

    private static string FormatCenterName(Center center)
    {
        return center switch
        {
            Center.Vesterbro => "Vesterbro",
            Center.Nørrebro => "Nørrebro",
            Center.Østerbro => "Østerbro",
            Center.AarhusC => "Aarhus C",
            Center.Kolding => "Kolding",
            _ => center.ToString()
        };
    }
}
