namespace hamster.Model;

public class Config
{
    public string EndpointURL { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }

    public List<BackupOperation> Operations { get; set; }
}