﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Update;

namespace UntisDesktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Logger.LogInformation("Application started with args: " + (e.Args.Length > 0 ? string.Join("; ", e.Args) : "No args available"));

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
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Logger.EndLogging(e.ApplicationExitCode);
        base.OnExit(e);
    }
}
