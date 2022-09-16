using System.Diagnostics;

namespace hamster.Utils;

public class PathUtils
{
    public static string ExecutingDirectory() => Path.GetDirectoryName(Process.GetCurrentProcess()?.MainModule?.FileName) 
                                                 ?? throw new DirectoryNotFoundException();

    public static string BuildBackupDir(string operationName)
    {
        var backupDir = Path.Combine(ExecutingDirectory(), operationName);
        
        if (!Directory.Exists(backupDir))
            Directory.CreateDirectory(backupDir);

        return backupDir;
    }

    public static string ConfigFilePath() => Path.Combine(ExecutingDirectory(), "config.json");
}