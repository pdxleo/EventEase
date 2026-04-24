namespace EventEase.Models;

public class EventRegistrationRecord
{
    public required int EventId { get; init; }

    public required string EventName { get; init; }

    public required string AttendeeName { get; init; }

    public required string Email { get; init; }

    // Retains the persisted property name while storing total party size.
    public int GuestCount { get; init; }

    public DateTimeOffset RegisteredAt { get; init; } = DateTimeOffset.UtcNow;

    public int TotalAttendees => GuestCount <= 0 ? 1 : GuestCount;
}
