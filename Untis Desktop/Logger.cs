using System;
using System.IO;
using System.Text;

namespace UntisDesktop;

internal static class Logger
{
    private static readonly string s_SaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Untis Desktop\Logs\";

    static Logger()
    {
        if (!Directory.Exists(s_SaveDirectory))
            Directory.CreateDirectory(s_SaveDirectory);

        using FileStream LogStream = new(s_SaveDirectory + "Untis Desktop.log", FileMode.OpenOrCreate, FileAccess.Write);
        LogStream.Position = LogStream.Length;
        LogStream.Write(Encoding.UTF8.GetBytes($"==================== {DateTime.UtcNow:s} Start logging ====================\n"));
    }

    public static void EndLogging(int exitCode)
    {
        using FileStream LogStream = new(s_SaveDirectory + "Untis Desktop.log", FileMode.OpenOrCreate, FileAccess.Write);
        LogStream.Write(Encoding.UTF8.GetBytes($"==================== {DateTime.UtcNow:s} End logging, Exit code: {exitCode} ====================\n"));
        LogStream.Dispose();
    }

    public static void LogInformation(string information)
    {
        using FileStream LogStream = new(s_SaveDirectory + "Untis Desktop.log", FileMode.OpenOrCreate, FileAccess.Write);
        LogStream.Write(Encoding.UTF8.GetBytes($"[{DateTime.UtcNow:O}](Information): {information}\n"));
        LogStream.Flush();
    }

    public static void LogWarning(string warning)
    {
        using FileStream LogStream = new(s_SaveDirectory + "Untis Desktop.log", FileMode.OpenOrCreate, FileAccess.Write);
        LogStream.Write(Encoding.UTF8.GetBytes($"[{DateTime.UtcNow:O}](Warning): {warning}\n"));
        LogStream.Flush();
    }

    public static void LogError(string error)
    {
        using FileStream LogStream = new(s_SaveDirectory + "Untis Desktop.log", FileMode.OpenOrCreate, FileAccess.Write);
        LogStream.Write(Encoding.UTF8.GetBytes($"[{DateTime.UtcNow:O}](Error): {error}\n"));
        LogStream.Flush();
    }

    public static void LogException(Exception ex)
    {
        using FileStream LogStream = new(s_SaveDirectory + "Untis Desktop.log", FileMode.OpenOrCreate, FileAccess.Write);
        LogStream.Write(Encoding.UTF8.GetBytes($"[{DateTime.UtcNow:O}](Exception): {ex.Source} was thrown! Message: {ex.Message}, Stack trace: {ex.StackTrace ?? "No stack trace available"}\n"));
        LogStream.Flush();
    }
}
