using Data.Profiles;
using Data.Static;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UntisDesktop.Views;
using WebUntisAPI.Client;
using Data.Timetable;
using UntisDesktop.ViewModels;
using UntisDesktop.Extensions;
using System.Timers;
using System.Threading;
using Timer = System.Timers.Timer;
using System.Windows.Threading;

namespace UntisDesktop;

public partial class App : Application
{
    public static WebUntisClient? Client { get; set; }

    private readonly Timer _timer = new(TimeSpan.FromMinutes(1));

    private async void UpdateClientTokenAsync(object? sender, ElapsedEventArgs e)
    {
        bool isAnyOffline = Dispatcher.Invoke(() => Windows.Cast<Window>().Where(w => w.DataContext is IWindowViewModel).Any(w => ((IWindowViewModel)w.DataContext).IsOffline));

        if (!Client?.LoggedIn ?? true || isAnyOffline)
            return;

        try
        {
            if (e.SignalTime.AddMinutes(1.5) >= Client!.SessionExpires.ToLocalTime())
            {
                await Client!.ReloadSessionFixAsync();
                Logger.LogInformation("Session token updated");
            }
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() =>
            {
                IWindowViewModel viewModel = (IWindowViewModel)Windows.Cast<Window>().FirstOrDefault(w => w.IsActive, MainWindow).DataContext;
                ex.HandleWithDefaultHandler(viewModel, "Update client token");
            });
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Exception ex = (Exception)e.ExceptionObject;
            Logger.LogException(ex);

            if (e.IsTerminating)
                MessageBox.Show($"An unhandled exception: {ex.Source}\nMessage: {ex.Message}\nStack trace: {ex.StackTrace}", "unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
        };
        Logger.LogInformation("Application started with args: " + (e.Args.Length > 0 ? string.Join(';', e.Args) : "No args available"));

#if RELEASE
        ProcessStartInfo updateStartInf = new()     // Update checking
        {
            FileName = Environment.CurrentDirectory + @"\Update.exe",
            Arguments = "no_latest_version_notification",
            RedirectStandardOutput = true
        };
        Process? updateProcess = Process.Start(updateStartInf);
        if (updateProcess is Process process)
        {
            switch (process.StandardOutput.ReadLine())
            {
                case "update_begin":
                    Logger.LogInformation("Update check: update found");
                    Shutdown(0);
                    break;
                case "update_denied":
                case "on_latest":
                    Logger.LogInformation("Update check: no update");
                    break;
                case "update_error":
                    Logger.LogError("Update check was failed");
                    break;
                default:
                    Logger.LogWarning("Update check: invalid result");
                    break;
            }
        }
        else
            Logger.LogError("Update check could not be started!");
#endif

        if (!ProfileCollection.s_DefaultInstance.Any())
            new LoginWindow().Show();
        else
        {
            Exception? exception = null;
            try
            {
                ProfileFile activeProfile = ProfileCollection.GetActiveProfile();

                Client = await activeProfile.LoginAsync(CancellationToken.None);
                _ = Task.Run(async () => await ProfileCollection.SetActiveProfileAsync(activeProfile));
            }
            catch (UnauthorizedAccessException)     // Bad credentials
            {
                // TODO: Bad credentials
                Logger.LogInformation("Profile deletion in cause of bad credentials");
            }
            catch (Exception ex)
            {
                exception = ex;
                Logger.LogInformation("Started without WU connection");
            }

            _timer.Elapsed += UpdateClientTokenAsync;
            _timer.Start();

            MainWindow = new MainWindow(exception is not null);
            MainWindow.Show();
        }
    }

    protected async override void OnExit(ExitEventArgs e)
    {
        await (Client?.LogoutAsync() ?? Task.CompletedTask);

        _timer.Stop();
        _timer.Dispose();

        Logger.EndLogging(e.ApplicationExitCode);
        base.OnExit(e);
    }
}
