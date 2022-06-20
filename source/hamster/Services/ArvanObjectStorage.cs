using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
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

    public async Task<bool> BucketExists(string bucketName)
    {
        return await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
    }
}