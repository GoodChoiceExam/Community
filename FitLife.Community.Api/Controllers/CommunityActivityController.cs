using FitLife.Community.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLife.Community.Api.Controllers;

// Eksponerer endpoint til at hente seneste aktivitet på tværs af alle communities.
// Bruges af frontend til at vise et aktivitetsfeed.
[ApiController]
[Authorize]
[Route("api/community/activity")]
public class CommunityActivityController : ControllerBase
{
    private readonly ICommunityService _communityService;
    private readonly ILogger<CommunityActivityController> _logger;

    public CommunityActivityController(ICommunityService communityService, ILogger<CommunityActivityController> logger)
    {
        _communityService = communityService;
        _logger = logger;
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentActivity([FromQuery] int take = 20)
    {
        _logger.LogInformation("Fetching recent community activity");
        return Ok(await _communityService.GetRecentActivityAsync(take));
    }
}
