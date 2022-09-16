namespace hamster.DTO;

public class BackupOperationDto
{
    public string Name { get; set; }
    public bool PersianDate { get; set; }
    public string BucketName { get; set; }
    public string RemoteFileName { get; set; }
    public string Command { get; set; }
}