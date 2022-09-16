using hamster.Utils;

namespace hamster.Model;

public class BackupOperation
{
    public string Name { get; set; }
    public string UnixCommand { get; set; }
    public string WindowsCommand { get; set; }
    public bool PersianDate { get; set; }
    public string RemoteFileName
    {
        get => TranslateRemoteFileName(_remoteFileName); 
        set => _remoteFileName = value;
    }

    private string _remoteFileName;

    public string Command
    {
        get
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return TranslateCommand(WindowsCommand);
                
                default:
                    return TranslateCommand(UnixCommand);
            }
        }
    }
    
    private string TranslateRemoteFileName(string fileName)
    {
        string date = PersianDate ?
            PersianDateUtils.Now() : DateTime.Now.ToString("yyyy.dd.MM-HH.mm.ss");
        
        return fileName
            .Replace("$date", date, StringComparison.OrdinalIgnoreCase)
            .Replace("$name", Name, StringComparison.OrdinalIgnoreCase);
    }

    private string TranslateCommand(string command)
    {
        return command
            .Replace("$Name", PathUtils.BackupDir(Name), StringComparison.OrdinalIgnoreCase)
            .Replace("\"", "\\\"");
    }
}