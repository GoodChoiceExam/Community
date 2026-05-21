using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FitLife.Community.Api.DTOs;
using FitLife.Community.Api.Models;
using FitLife.Community.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLife.Community.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/communities")]
public class CommunitiesController : ControllerBase
{
    private readonly ICommunityService _communityService;
    private readonly ILogger<CommunitiesController> _logger;

    public CommunitiesController(ICommunityService communityService, ILogger<CommunitiesController> logger)
    {
        _communityService = communityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetCommunities()
    {
        _logger.LogInformation("Fetching communities");
        return Ok(await _communityService.GetCommunitiesAsync());
    }
    
    [HttpGet("by-center/{center}")]
    public async Task<IActionResult> GetCommunityByCenter(Center center)
    {
        var community = await _communityService.GetCommunityByCenterAsync(center);
        if (community is null)
        {
            _logger.LogWarning("Community for center {Center} was not found", center);
            return NotFound();
        }

        return Ok(community);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCommunity(Guid id)
    {
        var community = await _communityService.GetCommunityByIdAsync(id);
        if (community is null)
        {
            _logger.LogWarning("Community {CommunityId} was not found", id);
            return NotFound();
        }

        return Ok(community);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateCommunity(CreateCommunityRequest request)
    {
        var community = await _communityService.CreateCommunityAsync(request);
        _logger.LogInformation("Created community {CommunityId}", community.Id);
        return CreatedAtAction(nameof(GetCommunity), new { id = community.Id }, community);
    }

    [HttpGet("{id:guid}/posts")]
    public async Task<IActionResult> GetPosts(Guid id)
    {
        var posts = await _communityService.GetPostsAsync(id);
        if (posts is null)
        {
            _logger.LogWarning("Cannot fetch posts; community {CommunityId} was not found", id);
            return NotFound();
        }

        return Ok(posts);
    }

    [HttpPost("{id:guid}/posts")]
    [Authorize]
    public async Task<IActionResult> CreatePost(Guid id, CreateCommunityPostRequest request)
    {
        var memberId = GetMemberId();
        if (memberId is null)
            return BadRequest("Token must contain a member id claim.");

        var authorName = GetAuthorName();
        var post = await _communityService.CreatePostAsync(id, memberId.Value, authorName, request);
        if (post is null)
        {
            _logger.LogWarning("Cannot create post; community {CommunityId} was not found", id);
            return NotFound();
        }

        _logger.LogInformation("Created community post {PostId} in community {CommunityId}", post.Id, id);
        return Created($"/api/communities/{id}/posts/{post.Id}", post);
    }

    private Guid? GetMemberId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("memberId")
                    ?? User.FindFirstValue("member_id");

        return Guid.TryParse(value, out var memberId) ? memberId : null;
    }

    [HttpGet("version")]
    public async Task<Dictionary<string, string>> GetVersion()
    {
        var properties = new Dictionary<string, string>();
        properties.Add("service", "FitLife Community API");
        var ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).ProductVersion;
        properties.Add("version", ver!);
        try
        {
            var hostName = System.Net.Dns.GetHostName();
            var ips = await System.Net.Dns.GetHostAddressesAsync(hostName);
            var ipa = ips.First().MapToIPv4().ToString();
            properties.Add("hosted-at-address", ipa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            properties.Add("hosted-at-address", "Could not resolve IP-address");
        }
        return properties;
    }

    private string GetAuthorName()
    {
        return User.FindFirstValue(JwtRegisteredClaimNames.Name)
               ?? User.FindFirstValue(ClaimTypes.Name)
               ?? User.FindFirstValue("name")
               ?? "FitLife Member";
    }
}
