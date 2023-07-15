using System;
using System.IO;
using System.Text;

namespace Data;

internal static class Logger
{
    private static readonly string s_SaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Untis Desktop\Logs\";
    private static readonly FileStream s_LogStream;

    static Logger()
    {
        if (!Directory.Exists(s_SaveDirectory))
            Directory.CreateDirectory(s_SaveDirectory);
        s_LogStream = new(s_SaveDirectory + "Data.log", FileMode.OpenOrCreate, FileAccess.Write);

        s_LogStream.Position = s_LogStream.Length;
        s_LogStream.Write(Encoding.UTF8.GetBytes($"==================== {DateTime.UtcNow:s} Start logging ====================\n"));
    }

    public static void EndLogging(int exitCode)
    {
        s_LogStream.Write(Encoding.UTF8.GetBytes($"==================== {DateTime.UtcNow:s} End logging, Exit code: {exitCode} ====================\n"));
        s_LogStream.Dispose();
    }

    public static void LogInformation(string information)
    {
        s_LogStream.Write(Encoding.UTF8.GetBytes($"[{DateTime.UtcNow:O}](Information): {information}\n"));
        s_LogStream.Flush();
    }

    public static void LogWarning(string warning)
    {
        s_LogStream.Write(Encoding.UTF8.GetBytes($"[{DateTime.UtcNow:O}](Warning): {warning}\n"));
        s_LogStream.Flush();
    }

    public static void LogError(string error)
    {
        s_LogStream.Write(Encoding.UTF8.GetBytes($"[{DateTime.UtcNow:O}](Error): {error}\n"));
        s_LogStream.Flush();
    }

    public static void LogException(Exception ex)
    {
        s_LogStream.Write(Encoding.UTF8.GetBytes($"[{DateTime.UtcNow:O}](Exception): {ex.Source} was thrown! Message: {ex.Message}, Stack trace: {ex.StackTrace ?? "No stack trace available"}\n"));
        s_LogStream.Flush();
    }
}
