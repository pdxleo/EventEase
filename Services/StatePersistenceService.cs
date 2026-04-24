using System.Text.Json;
using EventEase.Data;
using EventEase.Models;
using Microsoft.JSInterop;

namespace EventEase.Services;

public class StatePersistenceService
{
    private const string StorageKey = "eventease.sessionState.v1";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IJSRuntime jsRuntime;
    private readonly UserSessionTracker sessionTracker;
    private readonly AttendanceTracker attendanceTracker;
    private bool isInitialized;

    public StatePersistenceService(
        IJSRuntime jsRuntime,
        UserSessionTracker sessionTracker,
        AttendanceTracker attendanceTracker)
    {
        this.jsRuntime = jsRuntime;
        this.sessionTracker = sessionTracker;
        this.attendanceTracker = attendanceTracker;
    }

    public async Task InitializeAsync()
    {
        if (isInitialized)
        {
            return;
        }

        try
        {
            var json = await jsRuntime.InvokeAsync<string?>("eventEaseStorage.get", StorageKey);

            if (!string.IsNullOrWhiteSpace(json))
            {
                var state = JsonSerializer.Deserialize<PersistedAppState>(json, SerializerOptions);
                if (state is not null)
                {
                    sessionTracker.Restore(state.Session);
                    attendanceTracker.Restore(state.Registrations);
                    EventRepository.RestoreEvents(state.Events);
                }
            }
        }
        catch (JSException)
        {
            // Ignore browser storage failures and continue with fresh in-memory state.
        }

        isInitialized = true;
    }

    public async Task SaveAsync()
    {
        if (!isInitialized)
        {
            return;
        }

        var state = new PersistedAppState
        {
            Session = sessionTracker.ToState(),
            Registrations = attendanceTracker.GetAllRegistrations().ToList(),
            Events = EventRepository.GetAll().ToList()
        };

        var json = JsonSerializer.Serialize(state, SerializerOptions);
        await jsRuntime.InvokeVoidAsync("eventEaseStorage.set", StorageKey, json);
    }

    public async Task ClearAsync(string currentRoute = "/")
    {
        sessionTracker.Reset(currentRoute);
        attendanceTracker.Clear();
        EventRepository.ClearCustomEvents();

        try
        {
            await jsRuntime.InvokeVoidAsync("eventEaseStorage.remove", StorageKey);
        }
        catch (JSException)
        {
            // Ignore browser storage failures and continue with fresh in-memory state.
        }

        isInitialized = true;
    }
}