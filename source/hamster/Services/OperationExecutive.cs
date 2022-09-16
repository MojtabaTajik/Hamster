using hamster.DTO;
using hamster.Utils;
using Microsoft.Extensions.Logging;

namespace hamster.Services;

public class OperationExecutive
{
    private readonly UploadFileUtils _uploadFileUtils;
    private readonly CompressUtils _compressUtils;
    private readonly ILogger<OperationExecutive> _logger;

    public OperationExecutive(ILogger<OperationExecutive> logger, UploadFileUtils uploadFileUtils, CompressUtils compressUtils)
    {
        _logger = logger;
        _uploadFileUtils = uploadFileUtils;
        _compressUtils = compressUtils;
    }

    public async Task<bool> Execute(BackupOperationDto operation)
    {
        try
        {
            _logger.LogInformation("Executing operation : {OperationName}", operation.Name);

            string backupDir = PathUtils.BuildBackupDir(operation.Name);

            // Execute operation
            string result = await ProcessUtils.ExecuteBashCommand(operation.Command);
            _logger.LogInformation("Execute result => {Result}", result);
            
            // Check operation execution generate any backup file or not
            if (Directory.GetFiles(backupDir).Length <= 0)
            {
                _logger.LogInformation("Backup command finished without generating any file in backup dir");
                return false;
            }
            
            _logger.LogInformation("Backup done, compressing backup file ...");
            
            // Compress backup directory & split into parts if needed
            long maxPartSize = 5 * (long)Math.Pow(2, 30);

            string zipFilePartsStoragePath = Path.Combine(Path.GetTempPath(), operation.Name);
            var compressedFiles = _compressUtils.CompressDirectory(backupDir, zipFilePartsStoragePath,
                operation.RemoteFileName, maxPartSize);

            if (!compressedFiles.Any())
            {
                _logger.LogInformation("There is no file in => {ZipFilePartsStoragePath} to upload", zipFilePartsStoragePath);
                return false;
            }
            
            // Upload zip files (parts)
            int fileCounter = 0;
            foreach (var compressedFile in compressedFiles)
            {   
                fileCounter++;
                string fileName = Path.GetFileName(compressedFile);
                
                _logger.LogInformation("Uploading [{FileCounter}/{CompressedFilesCount}] => {FileName}",
                    fileCounter, compressedFiles.Count, fileName);
                
                bool uploadResult = await _uploadFileUtils.UploadFile(operation.BucketName, fileName, compressedFile);

                if (!uploadResult)
                {
                    _logger.LogError("Upload [{FileName}] failed", fileName);
                    return false;
                }
            }

            // Clean up files
            _logger.LogInformation("Cleanup up backup dir => {BackupDir}", backupDir);
            new DirectoryInfo(backupDir).Delete(true);
            
            _logger.LogInformation("Cleanup up temp zip parts => {ZipFilePartsStoragePath}", zipFilePartsStoragePath);
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