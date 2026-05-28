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
