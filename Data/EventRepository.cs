using EventEase.Models;

namespace EventEase.Data;

public static class EventRepository
{
    private static readonly IReadOnlyList<EventItem> Events = new List<EventItem>
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

    public static IReadOnlyList<EventItem> GetAll() => Events;

    public static EventItem? GetById(int id) => Events.FirstOrDefault(e => e.Id == id);
}
