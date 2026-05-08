using MongoDB.Bson.Serialization.Attributes;

namespace FitLife.Community.Api.Models;

public class CenterCommunity
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Center Center { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
