using System;
using System.IO;

namespace WinFormsApp1.Core.Diagnostics;

public static class CrashLogger
{
    public static void Log(Exception ex)
    {
        string crashLogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FnuyIncrementalGame",
            "crashlog.txt");

        Directory.CreateDirectory(Path.GetDirectoryName(crashLogPath)!);
        string log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{Environment.NewLine}";
        File.AppendAllText(crashLogPath, log);
    }
}