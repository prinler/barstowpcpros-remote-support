using Avalonia;
using Desktop.Shared.Services;
using Remotely.Desktop.Shared.Services;
using Remotely.Desktop.Shared.Startup;
using Remotely.Desktop.UI;
using Remotely.Desktop.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Remotely.Desktop.Win.Startup;
using Remotely.Shared.Services;
using Remotely.Shared.Utilities;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading;
using Remotely.Desktop.Shared.Abstractions;
using Remotely.Desktop.UI.Startup;
using Remotely.Desktop.Win.Services;

namespace Remotely.Desktop.Win;

public class Program
{
    // Barstow PC Pros: this build always knows where home is.
    private const string DefaultServerUrl = "https://support.barstowpcpros.com/";
    private const string DefaultOrganizationId = "3c13de52-0b85-4f0e-8e41-2e084f05d59c";

    // This is needed for the visual designer to work.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();


    [SupportedOSPlatform("windows")]
    public static async Task Main(string[] args)
    {
        var version = AppVersionHelper.GetAppVersion();
        var logger = new FileLogger("Remotely_Desktop", version, "Program.cs");
        var filePath = Environment.ProcessPath ?? Environment.GetCommandLineArgs().First();
        var serverUrl = Debugger.IsAttached ? "https://localhost:5001" : DefaultServerUrl;
        var getEmbeddedResult =  EmbeddedServerDataProvider.Instance.TryGetEmbeddedData(filePath);
        if (getEmbeddedResult.IsSuccess)
        {
            serverUrl = getEmbeddedResult.Value.ServerUrl.AbsoluteUri;
        }
        else
        {
            logger.LogInformation("No embedded server data in file name. Using built-in server {url}.", DefaultServerUrl);
        }
        var services = new ServiceCollection();

        services.AddSingleton<IEmbeddedServerDataProvider>(EmbeddedServerDataProvider.Instance);

        services.AddRemoteControlXplat();
        services.AddRemoteControlUi();
        services.AddRemoteControlWindows();

        services.AddLogging(builder =>
        {
            if (EnvironmentHelper.IsDebug)
            {
                builder.SetMinimumLevel(LogLevel.Debug);
            }
            builder.AddProvider(new FileLoggerProvider("Remotely_Desktop", version));
        });

        var provider = services.BuildServiceProvider();

        var appState = provider.GetRequiredService<IAppState>();

        if (getEmbeddedResult.IsSuccess)
        {
            appState.OrganizationId = getEmbeddedResult.Value.OrganizationId;
            appState.Host = getEmbeddedResult.Value.ServerUrl.AbsoluteUri;
        }

        if (string.IsNullOrWhiteSpace(appState.OrganizationId))
        {
            appState.OrganizationId = DefaultOrganizationId;
        }

        if (string.IsNullOrWhiteSpace(appState.Host))
        {
            appState.Host = serverUrl;
        }

        if (appState.ArgDict.TryGetValue("org-id", out var orgId))
        {
            appState.OrganizationId = orgId;
        }

        var result = await provider.UseRemoteControlClient(
            args,
            "Barstow PC Pros Remote Support.",
            serverUrl,
            false);

        if (!result.IsSuccess)
        {
            logger.LogError(result.Exception, "Failed to start remote control client.");
            Environment.Exit(1);
        }

        var dispatcher = provider.GetRequiredService<IUiDispatcher>();

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, dispatcher.ApplicationExitingToken);
        }
        catch (OperationCanceledException) { }

        // Output type is WinExe, so we need to explicitly exit.
        Environment.Exit(0);
    }
}