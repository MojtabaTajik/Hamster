using System.IO.Compression;

namespace hamster.Utils;

public class CompressUtils
{
    public List<string> CompressDirectoryInParts(string sourceDir, string destPath, string fileName, long partSize)
    {
        if (!Directory.Exists(sourceDir))
            return new List<string>();

        if (!Directory.Exists(destPath))
            Directory.CreateDirectory(destPath);
        
        var mainZipFilePath = Path.Combine(destPath, Path.GetRandomFileName());
        ZipFile.CreateFromDirectory(sourceDir, mainZipFilePath);
        if (!File.Exists(mainZipFilePath))
            return new List<string>();

        if (new FileInfo(mainZipFilePath).Length <= partSize)
        {
            return new List<string>
            {
                mainZipFilePath
            };
        }

        return SplitFile(mainZipFilePath, partSize, destPath, fileName);
    }

    private List<string> SplitFile(string inputFile, long partSize, string destPath, string baseFileName)
    {
        var result = new List<string>();
        
        int partNo = 0;

        using (var mainZipFileStream = new FileStream(inputFile, FileMode.Open))
        {
            var filePosition = 0;

            while (mainZipFileStream.Position < mainZipFileStream.Length)
            {
                partNo++;
                string fileNameWithPartNo = $"{baseFileName}-Part{partNo}";
                string filePath = Path.Combine(destPath, fileNameWithPartNo);

                // Calc last loop part size
                var tempPartSize = partSize;
                if (mainZipFileStream.Position + partSize > mainZipFileStream.Length)
                {
                    tempPartSize = mainZipFileStream.Length - mainZipFileStream.Position;
                }

                using (var partFileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    var buffer = new byte[tempPartSize];
                    var readCount = mainZipFileStream.Read(buffer);
                    partFileStream.Write(buffer);
                }

                result.Add(filePath);
            }
        }

        return result;
    }
}