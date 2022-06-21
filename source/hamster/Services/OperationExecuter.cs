using System.IO.Compression;
using hamster.Model;
using hamster.Utils;
using Microsoft.Extensions.Logging;

namespace hamster.Services;

public class OperationExecuter
{
    private readonly Config _config;
    private readonly UploadFileUtils _uploadFileUtils;
    private readonly ILogger<OperationExecuter> _logger;

    public OperationExecuter(Config config, ILogger<OperationExecuter> logger, UploadFileUtils uploadFileUtils)
    {
        _config = config;
        _logger = logger;
        _uploadFileUtils = uploadFileUtils;
    }

    public async Task<bool> ExecuteOperation(string operationName)
    {
        try
        {
            var operation = _config.Operations.FirstOrDefault(f => 
                f.Name.Equals(operationName, StringComparison.OrdinalIgnoreCase));

            if (operation == null)
            {
                _logger.LogCritical("No such operation with name {OperationName}", operationName);
                return false;
            }

            string backupDir = PathUtils.BackupDir(operation.Name);
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            // Execute operation
            string result = await ProcessUtils.ExecuteProcess(operation!.Command);
            _logger.LogInformation("Execute result => {Result}", result);
            
            if (Directory.GetFiles(backupDir).Length <= 0)
            {
                _logger.LogInformation("Backup command finished without generating any file in backup dir");
                return false;
            }
            
            _logger.LogInformation("Backup done, Uploading ...");

            string bucketName = $"{operation.Name}-Backup".ToLower();
            
            // Compress backup directory and upload it to bucket
            var tempZipFilePath = Path.Combine(Path.GetTempPath(), operation.RemoteFileName);
            if (File.Exists(tempZipFilePath))
                File.Delete(tempZipFilePath);

            ZipFile.CreateFromDirectory(backupDir, tempZipFilePath);

            bool uploadResult = await _uploadFileUtils.UploadFile(bucketName, operation.RemoteFileName, tempZipFilePath);
            _logger.LogInformation(uploadResult ? "Upload file success" : "Upload file failed");
            
            File.Delete(tempZipFilePath);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
    }
}