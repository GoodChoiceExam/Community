using MongoDB.Bson.Serialization.Attributes;

namespace FitLife.Community.Api.Models;

// Repræsenterer en community-gruppe tilknyttet et specifikt FitLife-center. Gemmes i MongoDB.
public class CenterCommunity
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Center Center { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
