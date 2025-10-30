using System;
using System.IO;

namespace WinFormsApp1.Core.Persistence;

public static class SaveFileCoordinator
{
    public static (bool ok, string backupPath, Exception? error) TryBackup(string savePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(savePath))
                return (false, string.Empty, new ArgumentException("savePath is null or empty.", nameof(savePath)));

            if (!File.Exists(savePath))
                return (false, string.Empty, null);

            string dir = Path.GetDirectoryName(savePath)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string baseBackup = savePath + ".plaintext-bak";
            string backupPath = baseBackup;

            if (File.Exists(baseBackup))
            {
                string suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                backupPath = savePath + $".plaintext-bak.{suffix}";
            }

            File.Move(savePath, backupPath);
            return (true, backupPath, null);
        }
        catch (Exception ex)
        {
            return (false, string.Empty, ex);
        }
    }
}