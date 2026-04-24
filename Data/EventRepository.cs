using EventEase.Models;

namespace EventEase.Data;

public static class EventRepository
{
    private static readonly List<EventItem> Events = new()
    {
        new()
        {
            Id = 1,
            Name = "Executive Leadership Summit",
            Date = new DateTime(2026, 5, 14),
            Location = "Seattle Convention Center",
            Description = "A one-day summit focused on leadership strategy, growth planning, and team development."
        },
        new()
        {
            Id = 2,
            Name = "Annual Charity Gala",
            Date = new DateTime(2026, 6, 6),
            Location = "Grand Ballroom, Downtown Hotel",
            Description = "An evening gala featuring dinner, keynote speakers, and fundraising for local community programs."
        },
        new()
        {
            Id = 3,
            Name = "Product Launch Mixer",
            Date = new DateTime(2026, 7, 21),
            Location = "Harbor View Rooftop",
            Description = "A social networking event celebrating the launch of our newest product with live demos and refreshments."
        }
    };

    private static int nextId = 4;

    public static IReadOnlyList<EventItem> GetAll() => Events.ToList().AsReadOnly();

    public static EventItem? GetById(int id) => Events.FirstOrDefault(e => e.Id == id);

    public static EventItem AddEvent(EventItem newEvent)
    {
        if (newEvent.Id == 0)
        {
            newEvent.Id = nextId++;
        }
        else if (newEvent.Id >= nextId)
        {
            nextId = newEvent.Id + 1;
        }

        Events.Add(newEvent);
        return newEvent;
    }

    public static void RestoreEvents(List<EventItem> persistedEvents)
    {
        if (persistedEvents.Count == 0)
        {
            return;
        }

        // Clear existing non-seed events and add persisted ones
        var seedEventIds = new[] { 1, 2, 3 };
        Events.RemoveAll(e => !seedEventIds.Contains(e.Id));

        foreach (var evt in persistedEvents.Where(e => !seedEventIds.Contains(e.Id)))
        {
            if (!Events.Any(e => e.Id == evt.Id))
            {
                Events.Add(evt);
                if (evt.Id >= nextId)
                {
                    nextId = evt.Id + 1;
                }
            }
        }
    }

    public static void ClearCustomEvents()
    {
        var seedEventIds = new[] { 1, 2, 3 };
        Events.RemoveAll(e => !seedEventIds.Contains(e.Id));
        nextId = 4;
    }
}
