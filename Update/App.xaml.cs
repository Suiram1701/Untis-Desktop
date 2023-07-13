using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using Update.Localization;

namespace Update;

public partial class App : Application
{
    private const string s_RepoOwner = "Suiram1701";
    private const string s_RepoName = "Untis-Desktop";
    private const string s_AssemblyName = "Untis Desktop.exe";

    protected override async void OnStartup(StartupEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Exception ex = (Exception)e.ExceptionObject;
            Logger.LogException(ex);

            if (e.IsTerminating)
            {
                Console.Write("update_error");
                Logger.EndLogging(ex.HResult);
                Shutdown(ex.HResult);
            }
        };
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        base.OnStartup(e);
        Shutdown();

        // Check for updates
        using HttpClient client = new();
        using HttpRequestMessage request = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://api.github.com/repos/{s_RepoOwner}/{s_RepoName}/releases/latest"),
        };
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        request.Headers.Add("User-Agent", $"Updater/{Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0)} Application {s_RepoName}");

        using HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        JObject responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());

        Match versionMatch = Regex.Match(responseObject.Value<string>("name") ?? "0.0.0.0", @".*((?:\d+\.){2,3}\d+).*");
        Version latestVersion = Version.Parse(versionMatch.Groups[1].Value);
        Version assemblyVersion = Assembly.LoadFrom(s_AssemblyName).GetName().Version ?? new Version(0, 0, 0, 0);

        if (latestVersion > assemblyVersion)
        {
            Logger.LogInformation($"Update found from v{assemblyVersion.ToString(3)} to v{latestVersion.ToString(3)}");
            MessageBoxResult result = MessageBox.Show(LangHelper.GetString("App.Inf.Update", assemblyVersion.ToString(3), latestVersion.ToString(3)), LangHelper.GetString("App.Title"), MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Console.Write("update_begin");
                string location = Assembly.GetExecutingAssembly().Location;
                string[] assemblies = GetAssemblyNames(location.Remove(location.LastIndexOf(@"\"))).ToArray();
                foreach (Process process in Process.GetProcesses())     // Close all processes that are startet from any involved assembly
                    if (assemblies.Any(assembly => Regex.IsMatch(assembly, $@".+{process.ProcessName}\.(exe|dll)")))
                        process.Kill();

                JToken? binaryAsset = responseObject.Value<JArray>("assets")?.First(asset => Regex.IsMatch(asset.Value<string>("name") ?? string.Empty, @"binaries\.zip"));
                Uri downloadUrl = new(binaryAsset?.Value<string>("browser_download_url") ?? string.Empty);
                new UpdateWindow(downloadUrl).Show();
                return;
            }
            else
            {
                Console.Write("update_denied");
                Logger.LogInformation("Update from user denied");
            }
        }
        else
        {
            Console.WriteLine("on_latest");
            Logger.LogInformation($"On latest version: v{assemblyVersion.ToString(3)}");
            if (!e.Args.Any(arg => arg == "no_latest_version_notification"))
                MessageBox.Show(LangHelper.GetString("App.Inf.Latest", assemblyVersion.ToString(3)), LangHelper.GetString("App.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        Shutdown(0);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Logger.EndLogging(e.ApplicationExitCode);
        base.OnExit(e);
    }

    internal static IEnumerable<string> GetAssemblyNames(string path)
    {
        foreach (string filePath in Directory.GetFileSystemEntries(path))
        {
            if (filePath.EndsWith(".exe") || filePath.EndsWith(".dll"))
            {
                if (Regex.IsMatch(filePath, @"Update[^\\]*\.(?:dll|exe)"))
                    continue;

                yield return filePath;
                continue;
            }

            if (!filePath[filePath.LastIndexOf(@"\")..].Contains('.'))
                foreach (string file in GetAssemblyNames(filePath))
                    yield return file;
        }
    }
}