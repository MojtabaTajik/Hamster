using Amazon.S3;
using hamster.Model;

namespace hamster.Services;

public class ArvanObjectStorage
{
    private readonly Config _config;
    private readonly AmazonS3Client _s3Client;

    public ArvanObjectStorage(Config config)
    {
        _config = config;
        
        var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(_config.AccessKey, _config.SecretKey);
        var s3Config = new AmazonS3Config { ServiceURL = _config.EndpointURL };
        _s3Client = new AmazonS3Client(awsCredentials, s3Config);
    }
}