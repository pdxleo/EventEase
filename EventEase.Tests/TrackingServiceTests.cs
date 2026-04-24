using EventEase.Models;
using EventEase.Services;
using Xunit;

namespace EventEase.Tests;

public class TrackingServiceTests
{
    [Fact]
    public void AttendanceTracker_Register_UpdatesCountsAndRegistrations()
    {
        var tracker = new AttendanceTracker();

        tracker.Register(new EventRegistrationRecord
        {
            EventId = 1,
            EventName = "Executive Leadership Summit",
            AttendeeName = "Alex Morgan",
            Email = "alex@example.com",
            GuestCount = 2,
            RegisteredAt = new DateTimeOffset(2026, 4, 23, 12, 0, 0, TimeSpan.Zero)
        });

        tracker.Register(new EventRegistrationRecord
        {
            EventId = 1,
            EventName = "Executive Leadership Summit",
            AttendeeName = "Jamie Carter",
            Email = "jamie@example.com",
            GuestCount = 1,
            RegisteredAt = new DateTimeOffset(2026, 4, 23, 13, 0, 0, TimeSpan.Zero)
        });

        Assert.Equal(2, tracker.GetRegistrationCount(1));
        Assert.Equal(3, tracker.GetAttendeeCount(1));

        var registrations = tracker.GetRegistrations(1);
        Assert.Equal(2, registrations.Count);
        Assert.Equal("Jamie Carter", registrations[0].AttendeeName);
    }

    [Fact]
    public void AttendanceTracker_GetAttendanceSummary_IncludesEventsWithoutRegistrations()
    {
        var tracker = new AttendanceTracker();

        tracker.Register(new EventRegistrationRecord
        {
            EventId = 2,
            EventName = "Annual Charity Gala",
            AttendeeName = "Taylor Brooks",
            Email = "taylor@example.com",
            GuestCount = 1
        });

        var summaries = tracker.GetAttendanceSummary();

        Assert.Equal(3, summaries.Count);
        Assert.Contains(summaries, summary => summary.EventId == 1 && summary.RegistrationCount == 0);
        Assert.Contains(summaries, summary => summary.EventId == 2 && summary.AttendeeCount == 1);
    }

    [Fact]
    public void UserSessionTracker_RecordRegistration_PersistsProfileAndSessionStats()
    {
        var tracker = new UserSessionTracker();

        tracker.TrackRoute("/events/1/register");
        tracker.RecordRegistration(1, "  Jordan Lee  ", "  jordan@example.com  ");

        Assert.True(tracker.HasProfile);
        Assert.Equal("Jordan Lee", tracker.UserName);
        Assert.Equal("jordan@example.com", tracker.Email);
        Assert.Equal("/events/1/register", tracker.LastVisitedRoute);
        Assert.Equal(1, tracker.RegistrationCount);
        Assert.Equal(1, tracker.LastRegisteredEventId);
        Assert.NotNull(tracker.LastRegistrationAt);
    }

    [Fact]
    public void UserSessionTracker_Restore_RehydratesPersistedState()
    {
        var tracker = new UserSessionTracker();

        tracker.Restore(new UserSessionState
        {
            SessionId = "SESSION42",
            UserName = " Morgan Price ",
            Email = " morgan@example.com ",
            LastVisitedRoute = "/attendance",
            RegistrationCount = 3,
            LastRegisteredEventId = 2,
            LastRegistrationAt = new DateTimeOffset(2026, 4, 23, 14, 30, 0, TimeSpan.Zero)
        });

        Assert.Equal("SESSION42", tracker.SessionId);
        Assert.Equal("Morgan Price", tracker.UserName);
        Assert.Equal("morgan@example.com", tracker.Email);
        Assert.Equal("/attendance", tracker.LastVisitedRoute);
        Assert.Equal(3, tracker.RegistrationCount);
        Assert.Equal(2, tracker.LastRegisteredEventId);
        Assert.Equal(new DateTimeOffset(2026, 4, 23, 14, 30, 0, TimeSpan.Zero), tracker.LastRegistrationAt);
    }

    [Fact]
    public void AttendanceTracker_Restore_RehydratesRegistrationData()
    {
        var tracker = new AttendanceTracker();

        tracker.Restore([
            new EventRegistrationRecord
            {
                EventId = 3,
                EventName = "Product Launch Mixer",
                AttendeeName = "Casey Nguyen",
                Email = "casey@example.com",
                GuestCount = 1,
                RegisteredAt = new DateTimeOffset(2026, 4, 23, 9, 0, 0, TimeSpan.Zero)
            },
            new EventRegistrationRecord
            {
                EventId = 3,
                EventName = "Product Launch Mixer",
                AttendeeName = "Riley Chen",
                Email = "riley@example.com",
                GuestCount = 2,
                RegisteredAt = new DateTimeOffset(2026, 4, 23, 11, 0, 0, TimeSpan.Zero)
            }
        ]);

        Assert.Equal(2, tracker.GetRegistrationCount(3));
        Assert.Equal(3, tracker.GetAttendeeCount(3));
        Assert.Equal("Riley Chen", tracker.GetRegistrations(3)[0].AttendeeName);
    }

    [Fact]
    public void UserSessionTracker_Reset_ClearsPersistedProfileAndMetrics()
    {
        var tracker = new UserSessionTracker();

        tracker.RecordRegistration(2, "Avery Stone", "avery@example.com");
        tracker.Reset("/attendance");

        Assert.False(tracker.HasProfile);
        Assert.Equal("/attendance", tracker.LastVisitedRoute);
        Assert.Equal(0, tracker.RegistrationCount);
        Assert.Null(tracker.LastRegisteredEventId);
        Assert.Null(tracker.LastRegistrationAt);
    }
    [Fact]
    public void EventRegistrationRecord_WithZeroStoredPartySize_TreatsAsSingleAttendee()
    {
        var registration = new EventRegistrationRecord
        {
            EventId = 1,
            EventName = "Executive Leadership Summit",
            AttendeeName = "Jordan Lee",
            Email = "jordan@example.com",
            GuestCount = 0
        };

        Assert.Equal(1, registration.TotalAttendees);
    }
    [Fact]
    public void AttendanceTracker_Clear_RemovesAllRegistrations()
    {
        var tracker = new AttendanceTracker();

        tracker.Register(new EventRegistrationRecord
        {
            EventId = 1,
            EventName = "Executive Leadership Summit",
            AttendeeName = "Avery Stone",
            Email = "avery@example.com",
            GuestCount = 2
        });

        tracker.Clear();

        Assert.Empty(tracker.GetAllRegistrations());
        Assert.Equal(0, tracker.GetRegistrationCount(1));
        Assert.Equal(0, tracker.GetAttendeeCount(1));
    }
}
