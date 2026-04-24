namespace EventEase.Models;

public class EventAttendanceSummary
{
    public required int EventId { get; init; }

    public required string EventName { get; init; }

    public required DateTime EventDate { get; init; }

    public int RegistrationCount { get; init; }

    public int AttendeeCount { get; init; }

    public DateTimeOffset? LastRegistrationAt { get; init; }
}