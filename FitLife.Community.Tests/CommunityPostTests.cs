using FitLife.Community.Api.Models;

namespace FitLife.Community.Tests;

[TestFixture]
public class CommunityPostTests
{
    [Test]
    public void NewPost_HasIdAndCreatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var post = new CommunityPost
        {
            CommunityId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            AuthorName = "FitLife Member",
            Content = "Hej center"
        };

        Assert.Multiple(() =>
        {
            Assert.That(post.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(post.CreatedAt, Is.GreaterThan(before));
        });
    }
}
