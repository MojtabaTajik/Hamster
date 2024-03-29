using System.Diagnostics;

namespace hamster.Utils;

public static class ProcessUtils
{
    public static async Task<string> ExecuteBashCommand(string command)
    {
        using var proc = new Process();
        proc.StartInfo.FileName = OperatingSystemUtils.GetOsCommandEngine();
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
}