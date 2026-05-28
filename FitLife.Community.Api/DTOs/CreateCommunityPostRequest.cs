using System.ComponentModel.DataAnnotations;

namespace FitLife.Community.Api.DTOs;

// Indeholder de felter klienten sender når et nyt indlæg skal oprettes i et community.
public class CreateCommunityPostRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;
}
