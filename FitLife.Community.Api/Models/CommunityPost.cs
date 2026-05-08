using MongoDB.Bson.Serialization.Attributes;

namespace FitLife.Community.Api.Models;

public class CommunityPost
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CommunityId { get; set; }
    public Guid MemberId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
