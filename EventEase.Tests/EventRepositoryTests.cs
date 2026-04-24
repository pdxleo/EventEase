using EventEase.Data;
using EventEase.Models;
using Xunit;

namespace EventEase.Tests;

[CollectionDefinition("EventRepository Collection")]
public class EventRepositoryCollection : ICollectionFixture<EventRepositoryFixture>
{
    // This class defines the collection
}

public class EventRepositoryFixture : IDisposable
{
    public EventRepositoryFixture()
    {
        EventRepository.ClearCustomEvents();
    }

    public void Dispose()
    {
        EventRepository.ClearCustomEvents();
    }
}

[Collection("EventRepository Collection")]
public class EventRepositoryTests : IDisposable
{
    public EventRepositoryTests()
    {
        EventRepository.ClearCustomEvents();
    }

    public void Dispose()
    {
        EventRepository.ClearCustomEvents();
    }

    [Fact]
    public void GetAll_ReturnsAllEvents_IncludingSeedAndCustom()
    {
        // Arrange
        EventRepository.ClearCustomEvents();
        var seedCount = EventRepository.GetAll().Count;
        var newEvent = new EventItem
        {
            Name = "Test Event",
            Date = DateTime.Today.AddDays(7),
            Location = "Test Location",
            Description = "Test Description"
        };

        // Act
        EventRepository.AddEvent(newEvent);
        var allEvents = EventRepository.GetAll();

        // Assert
        Assert.Equal(seedCount + 1, allEvents.Count);
        Assert.Contains(allEvents, e => e.Name == "Test Event");
        
        // Cleanup
        EventRepository.ClearCustomEvents();
    }

    [Fact]
    public void AddEvent_GeneratesUniqueId_WhenIdIsZero()
    {
        // Arrange
        EventRepository.ClearCustomEvents();
        var initialCount = EventRepository.GetAll().Count;
        var event1 = new EventItem
        {
            Id = 0,
            Name = "Event 1",
            Date = DateTime.Today.AddDays(7),
            Location = "Location 1",
            Description = "Description 1"
        };
        var event2 = new EventItem
        {
            Id = 0,
            Name = "Event 2",
            Date = DateTime.Today.AddDays(14),
            Location = "Location 2",
            Description = "Description 2"
        };

        // Act
        var created1 = EventRepository.AddEvent(event1);
        var created2 = EventRepository.AddEvent(event2);
        var allEvents = EventRepository.GetAll();

        // Assert
        Assert.NotEqual(0, created1.Id);
        Assert.NotEqual(0, created2.Id);
        Assert.NotEqual(created1.Id, created2.Id);
        Assert.Equal(initialCount + 2, allEvents.Count);

        // Cleanup
        EventRepository.ClearCustomEvents();
    }

    [Fact]
    public void AddEvent_PreservesProvidedId()
    {
        // Arrange
        EventRepository.ClearCustomEvents();
        var eventWithId = new EventItem
        {
            Id = 100,
            Name = "Event with ID",
            Date = DateTime.Today.AddDays(7),
            Location = "Test Location",
            Description = "Test Description"
        };

        // Act
        var created = EventRepository.AddEvent(eventWithId);
        var retrieved = EventRepository.GetById(100);

        // Assert
        Assert.Equal(100, created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Event with ID", retrieved.Name);

        // Cleanup
        EventRepository.ClearCustomEvents();
    }

    [Fact]
    public void GetById_ReturnsNullForNonexistentEvent()
    {
        // Arrange & Act
        var result = EventRepository.GetById(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void RestoreEvents_AddsPersistedCustomEventsWithoutDuplicates()
    {
        // Arrange
        EventRepository.ClearCustomEvents();
        var seedCount = EventRepository.GetAll().Count;
        var persistedEvents = new List<EventItem>
        {
            new()
            {
                Id = 10,
                Name = "Persisted Event 1",
                Date = DateTime.Today.AddDays(7),
                Location = "Location 1",
                Description = "Description 1"
            },
            new()
            {
                Id = 11,
                Name = "Persisted Event 2",
                Date = DateTime.Today.AddDays(14),
                Location = "Location 2",
                Description = "Description 2"
            }
        };

        // Act
        EventRepository.RestoreEvents(persistedEvents);
        var allEvents = EventRepository.GetAll();

        // Assert
        Assert.Equal(seedCount + 2, allEvents.Count);
        Assert.NotNull(EventRepository.GetById(10));
        Assert.NotNull(EventRepository.GetById(11));

        // Add same events again and verify no duplicates
        EventRepository.RestoreEvents(persistedEvents);
        var eventAfterRestore = EventRepository.GetAll();
        Assert.Equal(seedCount + 2, eventAfterRestore.Count);

        // Cleanup
        EventRepository.ClearCustomEvents();
    }

    [Fact]
    public void RestoreEvents_IgnoresSeedEvents()
    {
        // Arrange
        EventRepository.ClearCustomEvents();
        var beforeCount = EventRepository.GetAll().Count;
        var seedAndCustom = new List<EventItem>
        {
            new() { Id = 1, Name = "Seed Event", Date = DateTime.Today.AddDays(1), Location = "Seed", Description = "Seed" },
            new() { Id = 12, Name = "Custom Event", Date = DateTime.Today.AddDays(7), Location = "Custom", Description = "Custom" }
        };

        // Act
        EventRepository.RestoreEvents(seedAndCustom);
        var afterCount = EventRepository.GetAll().Count;

        // Assert - Only custom event should be added, not the seed
        Assert.Equal(beforeCount + 1, afterCount);
        Assert.NotNull(EventRepository.GetById(12));

        // Cleanup
        EventRepository.ClearCustomEvents();
    }

    [Fact]
    public void ClearCustomEvents_RemovesOnlyCustomEventsPreservesSeed()
    {
        // Arrange
        EventRepository.ClearCustomEvents();
        var seedCount = EventRepository.GetAll().Count;
        var customEvent = new EventItem
        {
            Name = "Custom Event",
            Date = DateTime.Today.AddDays(7),
            Location = "Test Location",
            Description = "Test Description"
        };
        EventRepository.AddEvent(customEvent);

        var beforeClear = EventRepository.GetAll().Count;

        // Act
        EventRepository.ClearCustomEvents();
        var afterClear = EventRepository.GetAll().Count;

        // Assert
        Assert.Equal(seedCount + 1, beforeClear);
        Assert.Equal(seedCount, afterClear);
        Assert.Equal(3, seedCount); // Verify 3 seed events remain
    }
}
