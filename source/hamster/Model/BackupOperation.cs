using hamster.Utils;

namespace hamster.Model;

public class BackupOperation
{
    public string Bucket { get; set; }
    public string Name { get; set; }
    public string Command { get; set; }
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
}