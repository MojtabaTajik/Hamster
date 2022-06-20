namespace hamster.Model;

public class BackupOperation
{
    public string Bucket { get; set; }
    public string Name { get; set; }
    public string Command { get; set; }
    public string BackupDir { get; set; }
    public string RemoteDir { get; set; }
}