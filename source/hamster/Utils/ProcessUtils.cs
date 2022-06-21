using System.Diagnostics;

namespace hamster.Utils;

public class ProcessUtils
{
    public static async Task<string> ExecuteProcess(string command)
    {
        using var proc = new Process();
        proc.StartInfo.FileName = GetOsCommandEngine();
        proc.StartInfo.Arguments = "-c \"" + command + "\"";
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;
        proc.Start();
        
        string result = string.Empty;
        result += proc.StandardOutput.ReadToEnd();
        result += proc.StandardError.ReadToEnd();

        await proc.WaitForExitAsync();

        return result;
    }

    private static string GetOsCommandEngine()
    {
        var os = Environment.OSVersion.Platform;

        switch (os)
        {
            case PlatformID.Win32NT:
                return "cmd.exe";
            
            default:
                return "/bin/bash";
        }
    }
}