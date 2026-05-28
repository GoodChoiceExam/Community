using System.ComponentModel.DataAnnotations;
using FitLife.Community.Api.DTOs;

namespace FitLife.Community.Tests;

// Tester at DataAnnotations-valideringen på CreateCommunityPostRequest virker korrekt.
[TestFixture]
public class CreateCommunityPostRequestTests
{
    [Test]
    public void Validate_WhenContentIsEmpty_ReturnsError()
    {
        var request = new CreateCommunityPostRequest { Content = "" };

        var results = Validate(request);

        Assert.That(results, Is.Not.Empty);
    }

    [Test]
    public void Validate_WhenContentIsTooLong_ReturnsError()
    {
        var request = new CreateCommunityPostRequest { Content = new string('a', 501) };

        var results = Validate(request);

        Assert.That(results, Is.Not.Empty);
    }

    [Test]
    public void Validate_WhenContentIsValid_ReturnsNoErrors()
    {
        var request = new CreateCommunityPostRequest { Content = "Kommer nogen til hold i aften?" };

        var results = Validate(request);

        Assert.That(results, Is.Empty);
    }

    private static List<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);
        return results;
    }
}
