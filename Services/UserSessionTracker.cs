using EventEase.Models;

namespace EventEase.Services;

public class UserSessionTracker
{
    public event Action? Changed;

    public string SessionId { get; private set; } = CreateSessionId();

    public string UserName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string LastVisitedRoute { get; private set; } = "/";

    public int RegistrationCount { get; private set; }

    public int? LastRegisteredEventId { get; private set; }

    public DateTimeOffset? LastRegistrationAt { get; private set; }

    public bool HasProfile => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Email);

    public UserSessionState ToState() => new()
    {
        SessionId = SessionId,
        UserName = UserName,
        Email = Email,
        LastVisitedRoute = LastVisitedRoute,
        RegistrationCount = RegistrationCount,
        LastRegisteredEventId = LastRegisteredEventId,
        LastRegistrationAt = LastRegistrationAt
    };

    public void Restore(UserSessionState? state)
    {
        if (state is null)
        {
            return;
        }

        SessionId = string.IsNullOrWhiteSpace(state.SessionId) ? CreateSessionId() : state.SessionId;
        UserName = state.UserName.Trim();
        Email = state.Email.Trim();
        LastVisitedRoute = string.IsNullOrWhiteSpace(state.LastVisitedRoute) ? "/" : state.LastVisitedRoute;
        RegistrationCount = Math.Max(0, state.RegistrationCount);
        LastRegisteredEventId = state.LastRegisteredEventId;
        LastRegistrationAt = state.LastRegistrationAt;
        NotifyChanged();
    }

    public void TrackRoute(string route)
    {
        LastVisitedRoute = string.IsNullOrWhiteSpace(route) ? "/" : route;
        NotifyChanged();
    }

    public void SaveProfile(string userName, string email)
    {
        UserName = userName.Trim();
        Email = email.Trim();
        NotifyChanged();
    }

    public void RecordRegistration(int eventId, string userName, string email)
    {
        SaveProfile(userName, email);
        RegistrationCount++;
        LastRegisteredEventId = eventId;
        LastRegistrationAt = DateTimeOffset.UtcNow;
    }

    public void Reset(string currentRoute = "/")
    {
        SessionId = CreateSessionId();
        UserName = string.Empty;
        Email = string.Empty;
        LastVisitedRoute = string.IsNullOrWhiteSpace(currentRoute) ? "/" : currentRoute;
        RegistrationCount = 0;
        LastRegisteredEventId = null;
        LastRegistrationAt = null;
        NotifyChanged();
    }

    private void NotifyChanged() => Changed?.Invoke();

    private static string CreateSessionId() => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}