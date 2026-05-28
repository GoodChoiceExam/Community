using FitLife.Community.Api.DTOs;
using FitLife.Community.Api.Models;
using FitLife.Community.Api.Repositories;
using FitLife.Community.Api.IntegrationEvents;

namespace FitLife.Community.Api.Services;

// Indeholder forretningslogik for communities og posts.
// Controlleren kalder servicen, som delegerer databaseoperationer til repository.
public class CommunityService : ICommunityService
{
    private readonly ICommunityRepository _repository;

    public CommunityService(ICommunityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CenterCommunity>> GetCommunitiesAsync()
    {
        return await _repository.GetCommunitiesAsync();
    }

    public async Task<CenterCommunity?> GetCommunityByIdAsync(Guid id)
    {
        return await _repository.GetCommunityByIdAsync(id);
    }

    public async Task<CenterCommunity> CreateCommunityAsync(CreateCommunityRequest request)
    {
        var community = new CenterCommunity
        {
            Center = request.Center!.Value,
            Name = request.Name.Trim()
        };

        return await _repository.AddCommunityAsync(community);
    }

    public async Task<List<CommunityPost>?> GetPostsAsync(Guid communityId)
    {
        var community = await _repository.GetCommunityByIdAsync(communityId);
        if (community is null)
            return null;

        return await _repository.GetPostsByCommunityAsync(communityId);
    }

    public async Task<CommunityPost?> CreatePostAsync(
        Guid communityId,
        Guid memberId,
        string authorName,
        CreateCommunityPostRequest request)
    {
        var community = await _repository.GetCommunityByIdAsync(communityId);
        if (community is null)
            return null;

        var post = new CommunityPost
        {
            CommunityId = communityId,
            MemberId = memberId,
            AuthorName = authorName.Trim(),
            Content = request.Content.Trim()
        };

        return await _repository.AddPostAsync(post);
    }

    public async Task<List<CommunityActivityDto>> GetRecentActivityAsync(int take = 20)
    {
        // Begrænser take til maks 100 så klienten ikke kan hente ubegrænset mange poster
        take = Math.Clamp(take, 1, 100);
        var posts = await _repository.GetRecentPostsAsync(take);

        if (posts.Count == 0)
            return [];

        // Henter de tilhørende communities i ét opslag i stedet for N opslag i en løkke
        var communityIds = posts.Select(p => p.CommunityId).Distinct();
        var communities = await _repository.GetCommunitiesByIdsAsync(communityIds);
        var communityById = communities.ToDictionary(c => c.Id);

        return posts
            .Where(p => communityById.ContainsKey(p.CommunityId))
            .Select(p =>
            {
                var community = communityById[p.CommunityId];
                return new CommunityActivityDto(
                    p.Id,
                    community.Id,
                    community.Center,
                    community.Name,
                    p.MemberId,
                    p.AuthorName,
                    p.Content,
                    p.CreatedAt);
            })
            .ToList();
    }

    // Når et nyt medlem oprettes oprettes der automatisk en community-gruppe for deres center,
    // men kun hvis den ikke allerede eksisterer
    public async Task HandleMemberCreatedAsync(MemberCreatedEvent memberCreatedEvent)
    {
        var center = MapCenter(memberCreatedEvent.PrimaryCenter);

        var existingCommunity = await _repository.GetCommunityByCenterAsync(center);
        if (existingCommunity is not null)
            return;

        var community = new CenterCommunity
        {
            Center = center,
            Name = $"{FormatCenterName(center)} Gruppe"
        };

        await _repository.AddCommunityAsync(community);
    }

    // Opretter automatisk en community-gruppe for centeret hvis den ikke findes endnu
    public async Task<CenterCommunity?> GetCommunityByCenterAsync(Center center)
    {
        var existing = await _repository.GetCommunityByCenterAsync(center);
        if (existing is not null)
            return existing;

        var community = new CenterCommunity
        {
            Center = center,
            Name = $"{FormatCenterName(center)} Gruppe"
        };

        return await _repository.AddCommunityAsync(community);
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
