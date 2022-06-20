using System.Diagnostics;
using hamster.Model;

namespace hamster.Services;

public class OperationExecuter
{
    private readonly Config _config;

    public OperationExecuter(Config config)
    {
        _config = config;
    }

    public string ExecuteOperation(string operationName)
    {
        try
        {
            var operation = _config.Operations.FirstOrDefault(f => 
                f.Name.ToLower().Equals(operationName.ToLower()));
            
            // Execute task
            string result = string.Empty;
            using var proc = new Process();
            proc.StartInfo.FileName = "/bin/bash";
            proc.StartInfo.Arguments = "-c \"" + operation!.Command + "\"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();

            result += proc.StandardOutput.ReadToEnd();
            result += proc.StandardError.ReadToEnd();

            proc.WaitForExit();

            return result;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}