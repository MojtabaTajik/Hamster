using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using hamster.Model;
using hamster.Services;
using hamster.Utils;

var configFilePath = PathUtils.ConfigFilePath();
if (!File.Exists(configFilePath))
{
    Console.WriteLine("Config file not found");
    return;
}

var configContent = File.ReadAllText(configFilePath);
var config = JsonSerializer.Deserialize<Config>(configContent);

if (config == null || args.Length < 1)
    return;

var serviceProvider = new ServiceCollection()
    .AddLogging(options =>
    {
        options.AddSimpleConsole(opt =>
        {
            opt.SingleLine = true;
            opt.TimestampFormat = "MM/dd/yyyy hh:mm:ss => ";
        });
    })
    .AddSingleton<OperationExecuter>()
    .AddScoped<AmazonS3ObjectStorage>()
    .AddScoped<UploadFileUtils>()
    .AddScoped<CompressUtils>()
    .AddScoped<NotifyUtils>()
    .AddSingleton(config)
    .BuildServiceProvider();

var logger = serviceProvider.GetService<ILogger<Program>>();
logger?.LogCritical("Hamster started");

var operationName = args[0];
var opExecuter = serviceProvider.GetService<OperationExecuter>();
var executeResult = await opExecuter?.ExecuteOperation(operationName)!;

if (executeResult)
{
    await NotifyUtils.SendNotification(operationName, "Backup success", "Backup complete successfully.", "information");
}
else
{
    await NotifyUtils.SendNotification(operationName, "Backup failed", "Failed to get backup.", "error");
}

logger?.LogCritical("Hamster done");