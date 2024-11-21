using Microsoft.Extensions.Configuration;

namespace GymTracker.Tests.Components.Analytics;

public abstract class TestContextBase : TestContext
{
    protected TestContextBase()
    {
        // Setup configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        Services.AddSingleton<IConfiguration>(configuration);

        // Add BlazorBootstrap services
        Services.AddBlazorBootstrap();

        // Set JSRuntime mode
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
