using System.ComponentModel.DataAnnotations;
using FitLife.Community.Api.Models;

namespace FitLife.Community.Api.DTOs;

public class CreateCommunityRequest
{
    [Required]
    public Center? Center { get; set; }

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}
