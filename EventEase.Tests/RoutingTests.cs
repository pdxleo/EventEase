using EventEase.Data;
using EventEase.Models;
using Xunit;

namespace EventEase.Tests;

public class RoutingTests
{
    [Fact]
    public void GetById_WithValidId_ReturnsEvent()
    {
        // Arrange
        int validId = 1;

        // Act
        var result = EventRepository.GetById(validId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(validId, result.Id);
        Assert.Equal("Executive Leadership Summit", result.Name);
    }

    [Fact]
    public void GetById_WithInvalidId_ReturnsNull()
    {
        // Arrange
        int invalidId = 999;

        // Act
        var result = EventRepository.GetById(invalidId);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void GetById_WithZeroAndNegativeIds_ReturnsNull(int id)
    {
        // Act
        var result = EventRepository.GetById(id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetById_WithAllValidIds_ReturnsExpectedEvents()
    {
        // Arrange
        var allEvents = EventRepository.GetAll();
        var validIds = allEvents.Select(e => e.Id).ToList();

        // Act & Assert
        foreach (var id in validIds)
        {
            var result = EventRepository.GetById(id);
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }
    }

    [Fact]
    public void GetAll_ReturnsAllEvents()
    {
        // Act
        var result = EventRepository.GetAll();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void GetAll_ReturnsEventsInConsistentOrder()
    {
        // Act
        var result1 = EventRepository.GetAll();
        var result2 = EventRepository.GetAll();

        // Assert - Verify consistent ordering
        Assert.Equal(result1.Count, result2.Count);
        for (int i = 0; i < result1.Count; i++)
        {
            Assert.Equal(result1[i].Id, result2[i].Id);
            Assert.Equal(result1[i].Name, result2[i].Name);
        }
    }

    [Fact]
    public void GetAll_ReturnsReadOnlyList()
    {
        // Act
        var result = EventRepository.GetAll();

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<EventItem>>(result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void EventRepository_ContainsExpectedEvents(int eventId)
    {
        // Act
        var events = EventRepository.GetAll();
        var eventItem = events.FirstOrDefault(e => e.Id == eventId);

        // Assert
        Assert.NotNull(eventItem);
        Assert.Equal(eventId, eventItem.Id);
    }

    [Fact]
    public void EventRepository_AllEventsHaveFutureOrTodayDates()
    {
        // Act
        var events = EventRepository.GetAll();

        // Assert
        foreach (var eventItem in events)
        {
            Assert.True(eventItem.Date.Date >= DateTime.Today, 
                $"Event '{eventItem.Name}' has a past date: {eventItem.Date}");
        }
    }

    [Fact]
    public void EventRepository_AllEventsHaveRequiredFields()
    {
        // Act
        var events = EventRepository.GetAll();

        // Assert
        foreach (var eventItem in events)
        {
            Assert.NotNull(eventItem.Name);
            Assert.NotEmpty(eventItem.Name);
            Assert.NotNull(eventItem.Location);
            Assert.NotEmpty(eventItem.Location);
            Assert.True(eventItem.Id > 0);
        }
    }
}
