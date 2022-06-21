using hamster.Utils;

namespace hamster.Model;

public class BackupOperation
{
    public string Name { get; set; }
    public string Command   
    {
        get => TranslateCommand(_command); 
        set => _command = value;
    }
    
    public string DirToBackup { get; set; }

    public string RemoteFileName
    {
        get => TranslateRemoteFileName(_remoteFileName); 
        set => _remoteFileName = value;
    }

    private string _remoteFileName;

    private string TranslateRemoteFileName(string fileName)
    {
        string date = PersianDateUtils.Now();

        return fileName
            .Replace("$date", date, StringComparison.OrdinalIgnoreCase)
            .Replace("$name", Name, StringComparison.OrdinalIgnoreCase);
    }

    private string _command;

    private string TranslateCommand(string command)
    {
        return command
            .Replace("$DirToBackup", DirToBackup, StringComparison.OrdinalIgnoreCase);
    }
}