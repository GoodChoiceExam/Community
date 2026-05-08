using System.ComponentModel.DataAnnotations;

namespace FitLife.Community.Api.DTOs;

public class CreateCommunityPostRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;
}
