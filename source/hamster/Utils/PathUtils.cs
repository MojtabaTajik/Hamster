using System.Diagnostics;
using System.Reflection;

namespace hamster.Utils;

public class PathUtils
{
    public static string ExecutingDirectory() => Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

    public static string BackupDir(string operationName) => Path.Combine(ExecutingDirectory(), operationName);

    public static string ConfigFilePath() => Path.Combine(ExecutingDirectory(), "config.json");
}