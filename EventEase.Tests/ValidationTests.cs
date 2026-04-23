using System.ComponentModel.DataAnnotations;
using EventEase.Models;
using Xunit;

namespace EventEase.Tests;

public class ValidationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }

    [Fact]
    public void EventItem_WithValidData_PassesValidation()
    {
        // Arrange
        var eventItem = new EventItem
        {
            Id = 1,
            Name = "Valid Event",
            Date = DateTime.Today.AddDays(7),
            Location = "Test Location",
            Description = "Test Description"
        };

        // Act
        var results = ValidateModel(eventItem);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void EventItem_WithEmptyName_FailsValidation()
    {
        // Arrange
        var eventItem = new EventItem
        {
            Id = 1,
            Name = string.Empty,
            Date = DateTime.Today.AddDays(7),
            Location = "Test Location",
            Description = "Test Description"
        };

        // Act
        var results = ValidateModel(eventItem);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(EventItem.Name)));
    }

    [Fact]
    public void EventItem_WithNameExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var eventItem = new EventItem
        {
            Id = 1,
            Name = new string('A', 101),
            Date = DateTime.Today.AddDays(7),
            Location = "Test Location",
            Description = "Test Description"
        };

        // Act
        var results = ValidateModel(eventItem);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(EventItem.Name)));
    }

    [Fact]
    public void EventItem_WithLocationExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var eventItem = new EventItem
        {
            Id = 1,
            Name = "Valid Event",
            Date = DateTime.Today.AddDays(7),
            Location = new string('A', 121),
            Description = "Test Description"
        };

        // Act
        var results = ValidateModel(eventItem);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(EventItem.Location)));
    }

    [Fact]
    public void EventItem_WithDescriptionExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var eventItem = new EventItem
        {
            Id = 1,
            Name = "Valid Event",
            Date = DateTime.Today.AddDays(7),
            Location = "Test Location",
            Description = new string('A', 501)
        };

        // Act
        var results = ValidateModel(eventItem);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(EventItem.Description)));
    }

    [Fact]
    public void EventItem_WithPastDate_FailsValidation()
    {
        // Arrange
        var eventItem = new EventItem
        {
            Id = 1,
            Name = "Past Event",
            Date = DateTime.Today.AddDays(-1),
            Location = "Test Location",
            Description = "Test Description"
        };

        // Act
        var results = ValidateModel(eventItem);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("past"));
    }

    [Fact]
    public void EventItem_WithTodayDate_PassesValidation()
    {
        // Arrange
        var eventItem = new EventItem
        {
            Id = 1,
            Name = "Today Event",
            Date = DateTime.Today,
            Location = "Test Location",
            Description = "Test Description"
        };

        // Act
        var results = ValidateModel(eventItem);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void EventItem_WithEmptyLocation_FailsValidation()
    {
        // Arrange
        var eventItem = new EventItem
        {
            Id = 1,
            Name = "Valid Event",
            Date = DateTime.Today.AddDays(7),
            Location = string.Empty,
            Description = "Test Description"
        };

        // Act
        var results = ValidateModel(eventItem);

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(EventItem.Location)));
    }

    [Fact]
    public void EventItem_WithEmptyDescription_PassesValidation()
    {
        // Arrange (Description is optional)
        var eventItem = new EventItem
        {
            Id = 1,
            Name = "Valid Event",
            Date = DateTime.Today.AddDays(7),
            Location = "Test Location",
            Description = string.Empty
        };

        // Act
        var results = ValidateModel(eventItem);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void EventItem_WithMaxValidLengths_PassesValidation()
    {
        // Arrange (test boundary conditions)
        var eventItem = new EventItem
        {
            Id = 1,
            Name = new string('A', 100),
            Date = DateTime.Today.AddDays(7),
            Location = new string('B', 120),
            Description = new string('C', 500)
        };

        // Act
        var results = ValidateModel(eventItem);

        // Assert
        Assert.Empty(results);
    }
}
