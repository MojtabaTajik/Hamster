using hamster.Services;
using Microsoft.Extensions.Logging;

namespace hamster.Utils;

public class UploadFileUtils
{
    private readonly ArvanObjectStorage _aos;
    private readonly ILogger<UploadFileUtils> _logger;

    public UploadFileUtils(ArvanObjectStorage aos, ILogger<UploadFileUtils> logger)
    {
        _aos = aos;
        _logger = logger;
    }

    public async Task<bool> UploadFile(string bucketName, string fileName, string filePath)
    {
        bool bucketExists = await _aos.BucketExists(bucketName);
        if (!bucketExists)
        {
            _logger.LogInformation("Bucket not exists, create it");
            bucketExists = await _aos.CreateBucketAsync(bucketName);
        }

        if (bucketExists)
        {
            return await _aos.UploadObjectAsync(bucketName, fileName, filePath);
        }

        return false;
    }
}