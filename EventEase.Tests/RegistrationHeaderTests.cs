using Bunit;
using EventEase.Layout;
using EventEase.Pages;
using EventEase.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Xunit;

namespace EventEase.Tests;

public class RegistrationHeaderTests : BunitContext
{
    [Fact]
    public void SubmitRegistration_UpdatesHeaderRegistrationCount()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        var sessionTracker = new UserSessionTracker();
        var attendanceTracker = new AttendanceTracker();

        Services.AddSingleton(sessionTracker);
        Services.AddSingleton(attendanceTracker);
        Services.AddSingleton<StatePersistenceService>();

        var host = Render<LayoutWithRegistrationHost>();

        host.Find("input.form-control").Change("Jordan Lee");
        host.FindAll("input.form-control")[1].Change("jordan@example.com");
        host.FindAll("input.form-control")[2].Change("1");
        host.Find("button[type='submit']").Click();

        host.WaitForAssertion(() =>
        {
            var registrationPill = host.FindAll(".session-pill")[2];
            Assert.Contains("Registrations", registrationPill.TextContent);
            Assert.Contains("1", registrationPill.TextContent);
        });

        Assert.Equal(1, sessionTracker.RegistrationCount);
        Assert.Equal(1, attendanceTracker.GetRegistrationCount(1));
    }

    private sealed class LayoutWithRegistrationHost : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<MainLayout>(0);
            builder.AddAttribute(1, nameof(LayoutComponentBase.Body), (RenderFragment)(bodyBuilder =>
            {
                bodyBuilder.OpenComponent<EventRegistration>(0);
                bodyBuilder.AddAttribute(1, nameof(EventRegistration.Id), 1);
                bodyBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        }
    }
}