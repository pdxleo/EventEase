namespace EventEase.Models;

public class PersistedAppState
{
    public UserSessionState Session { get; init; } = new();

    public List<EventRegistrationRecord> Registrations { get; init; } = [];

    public List<EventItem> Events { get; init; } = [];
}

public class UserSessionState
{
    public string SessionId { get; init; } = string.Empty;

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string LastVisitedRoute { get; init; } = "/";

    public int RegistrationCount { get; init; }

    public int? LastRegisteredEventId { get; init; }

    public DateTimeOffset? LastRegistrationAt { get; init; }
}