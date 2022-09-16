using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Ardalis.GuardClauses;
using hamster.DTO;
using Microsoft.Extensions.Logging;

namespace hamster.Services;

public class AmazonS3ObjectStorage
{
    private readonly AmazonS3Client _s3Client;
    private readonly ILogger<AmazonS3ObjectStorage> _logger;

    public AmazonS3ObjectStorage(ILogger<AmazonS3ObjectStorage> logger, HamsterConfigDto configDto)
    {
        _logger = logger;

        var awsCredentials = new BasicAWSCredentials(configDto.S3_AccessKey, configDto.S3_SecretKey);
        var s3Config = new AmazonS3Config { ServiceURL = configDto.S3_EndpointURL };
        _s3Client = new AmazonS3Client(awsCredentials, s3Config);
    }

    public async Task<bool> CreateBucketAsync(string bucketName)
    {
        try
        {
            Guard.Against.NullOrWhiteSpace(bucketName, nameof(bucketName));
            
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true,
            };

            var putBucketResponse = await _s3Client.PutBucketAsync(putBucketRequest);
            bool setLifecycleRuleResult = await SetBucketLifecycleRule(bucketName);
            return (putBucketResponse.HttpStatusCode == HttpStatusCode.OK) && setLifecycleRuleResult;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError("Error creating bucket: \'{ExMessage}\'", ex.Message);
            return false;
        }
    }

    public async Task<bool> BucketExists(string bucketName) => await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);

    private async Task<bool> CheckObjectExists(string bucketName, string objectName)
    {
        try
        {
            Guard.Against.NullOrWhiteSpace(bucketName, nameof(bucketName));
            Guard.Against.NullOrWhiteSpace(objectName, nameof(objectName));
            
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
        Guard.Against.NullOrWhiteSpace(bucketName, nameof(bucketName));
        Guard.Against.NullOrWhiteSpace(keyName, nameof(keyName));
        Guard.Against.NullOrWhiteSpace(filePath, nameof(filePath));
        
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
        try
        {
            int partSizeInMb = 200;
            long partSizeInByte = partSizeInMb * (long)Math.Pow(2, 20); // 200 MB

            long contentLength = new FileInfo(filePath).Length;

            var partCount = Math.Round((double)(contentLength / partSizeInByte), MidpointRounding.AwayFromZero);

            // Set part count to one for single part files
            if (contentLength > 0 && partCount == 0)
                partCount = 1;

            long partNo = 0;

            long filePosition = 0;
            for (int i = 1; filePosition < contentLength; i++)
            {
                partNo++;

                var remainingMb = ((contentLength - filePosition) / Math.Pow(2, 20));

                _logger.LogInformation("Uploading part {PartNo}/{PartCount} - Remaining size = ({RemainingMb} MB)"
                    , partNo, partCount, remainingMb);

                UploadPartRequest uploadRequest = new()
                {
                    BucketName = bucketName,
                    Key = keyName,
                    UploadId = initResponse.UploadId,
                    PartNumber = i,
                    PartSize = partSizeInByte,
                    FilePosition = filePosition,
                    FilePath = filePath,
                };

                // Track upload progress.
                uploadRequest.StreamTransferProgress += UploadPartProgressEventCallback;

                // Upload a part and add the response to our list.
                uploadResponses.Add(await _s3Client.UploadPartAsync(uploadRequest));

                filePosition += partSizeInByte;
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
            _logger.LogError("An AmazonS3Exception was thrown: {ExceptionMessage}, cleaning uploaded files",
                exception.Message);

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
    
    private async Task<bool> SetBucketLifecycleRule(string bucketName)
    {
        try
        {
            Guard.Against.NullOrWhiteSpace(bucketName, nameof(bucketName));
            
            var lifecycleConfiguration = new LifecycleConfiguration
            {
                Rules = new List<LifecycleRule>
                {
                    new LifecycleRule
                    {
                        Filter = new LifecycleFilter
                        {
                            LifecycleFilterPredicate = new LifecyclePrefixPredicate
                            {
                          
                            }
                        },
                        AbortIncompleteMultipartUpload = new LifecycleRuleAbortIncompleteMultipartUpload(){DaysAfterInitiation = 2},
                        Expiration = new LifecycleRuleExpiration { Days = 30 },
                        Status = "Enabled",
                    }
                }
            };

            var putLifecycleConfigurationRequest = new PutLifecycleConfigurationRequest
            {
                BucketName = bucketName,
                Configuration = lifecycleConfiguration
            };

            await _s3Client.PutLifecycleConfigurationAsync(putLifecycleConfigurationRequest);

            _logger.LogInformation("Lifecycle configuration added to {BucketName} bucket.", bucketName);
            return true;
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            _logger.LogError("Failed to set lifecycle rules => {EMessage}", amazonS3Exception.Message);
            return false;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to set lifecycle rules => {EMessage}", e.Message);
            return false;
        }
    }

    private void UploadPartProgressEventCallback(object? sender, StreamTransferProgressArgs e)
    {
        // Log percent done each 10 percent
        //if (e.PercentDone % 10 == 0)
            //_logger.LogInformation("{EPercentDone} %", e.PercentDone);
    }
}