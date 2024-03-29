namespace hamster.Model;

public class BackupOperation
{
    public string Name { get; set; }
    public string UnixCommand { get; set; }
    public string WindowsCommand { get; set; }
    public bool PersianDate { get; set; }
    public string BucketName { get; set; }
    public string RemoteFileName { get; set; }
}