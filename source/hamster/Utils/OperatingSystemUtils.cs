namespace hamster.Utils;

public class OperatingSystemUtils
{
    public static string GetOsCommandEngine()
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32NT:
                return "cmd.exe";
            
            default:
                return "/bin/bash";
        }
    }
}