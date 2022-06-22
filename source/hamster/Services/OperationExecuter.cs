using System.IO.Compression;
using hamster.Model;
using hamster.Utils;
using Microsoft.Extensions.Logging;

namespace hamster.Services;

public class OperationExecuter
{
    private readonly Config _config;
    private readonly UploadFileUtils _uploadFileUtils;
    private readonly CompressUtils _compressUtils;
    private readonly ILogger<OperationExecuter> _logger;

    public OperationExecuter(Config config, ILogger<OperationExecuter> logger, UploadFileUtils uploadFileUtils, CompressUtils compressUtils)
    {
        _config = config;
        _logger = logger;
        _uploadFileUtils = uploadFileUtils;
        _compressUtils = compressUtils;
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
            
            // Compress backup directory
            int maxAmazonS3BucketObjectSize = unchecked(5 * 1024 * 1024 * 1024);

            string zipFilePartsStoragePath = Path.Combine(Path.GetTempPath(), operation.Name);
            var compressedFiles = _compressUtils.CompressDirectoryInParts(backupDir, zipFilePartsStoragePath,
                operation.RemoteFileName, maxAmazonS3BucketObjectSize);

            // Upload zip files (parts)
            int partCounter = 0;
            foreach (var compressedFile in compressedFiles)
            {   
                partCounter++;
                string fileName = Path.GetFileName(compressedFile);
                
                _logger.LogInformation("Uploading [{PartCounter}/{CompressedFileLength}] => {FileName}",
                    partCounter, compressedFile.Length, fileName);
                
                bool uploadResult = await _uploadFileUtils.UploadFile(bucketName, fileName, compressedFile);

                if (!uploadResult)
                {
                    _logger.LogInformation("Upload [{FileName}] failed", fileName);
                }
            }

            // Clean up files
            _logger.LogInformation("Cleanup up backup dir ...");
            new DirectoryInfo(backupDir).Delete(true);
            
            _logger.LogInformation("Cleanup up temp zip parts ...");
            new DirectoryInfo(zipFilePartsStoragePath).Delete(true);
            
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("Global exception => {EMessage}", e.Message);
            return false;
        }
    }
}