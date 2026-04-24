using EventEase.Data;
using EventEase.Models;

namespace EventEase.Services;

public class AttendanceTracker
{
    private readonly Dictionary<int, List<EventRegistrationRecord>> registrationsByEvent = new();

    public void Register(EventRegistrationRecord registration)
    {
        if (!registrationsByEvent.TryGetValue(registration.EventId, out var registrations))
        {
            registrations = new List<EventRegistrationRecord>();
            registrationsByEvent[registration.EventId] = registrations;
        }

        registrations.Add(registration);
    }

    public int GetRegistrationCount(int eventId) =>
        registrationsByEvent.TryGetValue(eventId, out var registrations)
            ? registrations.Count
            : 0;

    public int GetAttendeeCount(int eventId) =>
        registrationsByEvent.TryGetValue(eventId, out var registrations)
            ? registrations.Sum(registration => registration.TotalAttendees)
            : 0;

    public IReadOnlyList<EventRegistrationRecord> GetRegistrations(int eventId) =>
        registrationsByEvent.TryGetValue(eventId, out var registrations)
            ? registrations.OrderByDescending(registration => registration.RegisteredAt).ToList()
            : [];

    public IReadOnlyList<EventRegistrationRecord> GetAllRegistrations() =>
        registrationsByEvent
            .Values
            .SelectMany(registrations => registrations)
            .OrderByDescending(registration => registration.RegisteredAt)
            .ToList();

    public void Restore(IEnumerable<EventRegistrationRecord>? registrations)
    {
        registrationsByEvent.Clear();

        if (registrations is null)
        {
            return;
        }

        foreach (var registration in registrations.OrderBy(registration => registration.RegisteredAt))
        {
            Register(registration);
        }
    }

    public void Clear()
    {
        registrationsByEvent.Clear();
    }

    public IReadOnlyList<EventAttendanceSummary> GetAttendanceSummary()
    {
        return EventRepository.GetAll()
            .Select(eventItem =>
            {
                registrationsByEvent.TryGetValue(eventItem.Id, out var registrations);
                registrations ??= [];

                return new EventAttendanceSummary
                {
                    EventId = eventItem.Id,
                    EventName = eventItem.Name,
                    EventDate = eventItem.Date,
                    RegistrationCount = registrations.Count,
                    AttendeeCount = registrations.Sum(registration => registration.TotalAttendees),
                    LastRegistrationAt = registrations
                        .OrderByDescending(registration => registration.RegisteredAt)
                        .FirstOrDefault()
                        ?.RegisteredAt
                };
            })
            .OrderBy(summary => summary.EventDate)
            .ToList();
    }
}