using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using hamster.Model;
using Microsoft.Extensions.Logging;

namespace hamster.Services;

public class ArvanObjectStorage
{
    private readonly Config _config;
    private readonly AmazonS3Client _s3Client;
    private readonly ILogger<ArvanObjectStorage> _logger;

    public ArvanObjectStorage(Config config, ILogger<ArvanObjectStorage> logger)
    {
        _config = config;
        _logger = logger;

        var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(_config.AccessKey, _config.SecretKey);
        var s3Config = new AmazonS3Config { ServiceURL = _config.EndpointURL };
        _s3Client = new AmazonS3Client(awsCredentials, s3Config);
    }

    public async Task<bool> CreateBucketAsync(string bucketName)
    {
        try
        {
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true,
            };

            var putBucketResponse = await _s3Client.PutBucketAsync(putBucketRequest);
            return putBucketResponse.HttpStatusCode == HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError("Error creating bucket: \'{ExMessage}\'", ex.Message);
            return false;
        }
    }

    public async Task<bool> BucketExists(string bucketName)
    {
        return await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
    }

    public async Task<GetACLResponse?> GetBucketACL(string bucketName)
    {
        try
        {
            return await _s3Client.GetACLAsync(new GetACLRequest
            {
                BucketName = bucketName,
            });
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex.Message);
            return null;
        }
    }

    public async Task<bool> CheckObjectExists(string bucketName, string objectName)
    {
        try
        {
            var metadataRequest = new GetObjectMetadataRequest()
            {
                BucketName = bucketName,
                Key = objectName
            };

            var fileMetadata = await _s3Client.GetObjectMetadataAsync(metadataRequest);
            return fileMetadata.HttpStatusCode == HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex.Message);
            return false;
        }
    }
    
    
    public async Task<bool> UploadObjectAsync(string bucketName, string keyName, string filePath)
    {
        // Create list to store upload part responses.
        List<UploadPartResponse> uploadResponses = new();

        // Setup information required to initiate the multipart upload.
        InitiateMultipartUploadRequest initiateRequest = new()
        {
            BucketName = bucketName,
            Key = keyName,
        };

        // Initiate the upload.
        InitiateMultipartUploadResponse initResponse =
            await _s3Client.InitiateMultipartUploadAsync(initiateRequest);

        // Upload parts.
        long contentLength = new FileInfo(filePath).Length;
        long partSize = 400 * (long)Math.Pow(2, 20); // 400 MB

        try
        {
            long filePosition = 0;
            for (int i = 1; filePosition < contentLength; i++)
            {
                UploadPartRequest uploadRequest = new()
                {
                    BucketName = bucketName,
                    Key = keyName,
                    UploadId = initResponse.UploadId,
                    PartNumber = i,
                    PartSize = partSize,
                    FilePosition = filePosition,
                    FilePath = filePath,
                };

                // Track upload progress.
                uploadRequest.StreamTransferProgress += UploadPartProgressEventCallback;

                // Upload a part and add the response to our list.
                uploadResponses.Add(await _s3Client.UploadPartAsync(uploadRequest));

                filePosition += partSize;
            }

            // Setup to complete the upload.
            CompleteMultipartUploadRequest completeRequest = new()
            {
                BucketName = bucketName,
                Key = keyName,
                UploadId = initResponse.UploadId,
            };
            completeRequest.AddPartETags(uploadResponses);

            // Complete the upload.
            CompleteMultipartUploadResponse completeUploadResponse =
                await _s3Client.CompleteMultipartUploadAsync(completeRequest);

            return await CheckObjectExists(bucketName, keyName);
        }
        catch (Exception exception)
        {
            _logger.LogError("An AmazonS3Exception was thrown: {ExceptionMessage}", exception.Message);

            // Abort the upload.
            AbortMultipartUploadRequest abortMpuRequest = new()
            {
                BucketName = bucketName,
                Key = keyName,
                UploadId = initResponse.UploadId,
            };
            await _s3Client.AbortMultipartUploadAsync(abortMpuRequest);
            return false;
        }
    }

    private void UploadPartProgressEventCallback(object? sender, StreamTransferProgressArgs e)
    {
        _logger.LogInformation("{ETransferredBytes}/{ETotalBytes}", e.TransferredBytes, e.TotalBytes);
    }
}