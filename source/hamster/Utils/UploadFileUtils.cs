using hamster.Services;

namespace hamster.Utils;

public class UploadFileUtils
{
    private readonly ArvanObjectStorage _aos;

    public UploadFileUtils(ArvanObjectStorage aos)
    {
        _aos = aos;
    }

    public async Task<bool> UploadFile(string bucketName, string fileName, string filePath)
    {
        bool bucketExists = await _aos.BucketExists(bucketName);
        if (!bucketExists)
        {
            bucketExists = await _aos.CreateBucketAsync(bucketName);
        }

        if (bucketExists)
        {
            var acl = await _aos.GetBucketACL(bucketName);
            var isFullControl = acl.AccessControlList.Grants.Exists(e => e.Permission.Value.Equals("FULL_CONTROL"));
            if (isFullControl)
            {
                return await _aos.UploadObjectAsync(bucketName, fileName, filePath);
            }
        }

        return false;
    }
}