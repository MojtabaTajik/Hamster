namespace hamster.Model;

public class ConfigFile
{
    public string S3_EndpointURL { get; set; }
    public string S3_AccessKey { get; set; }
    public string S3_SecretKey { get; set; }

    public List<BackupOperation> Operations { get; set; }
}