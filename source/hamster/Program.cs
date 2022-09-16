
using System.Text.Json;
using Ardalis.GuardClauses;
using AutoMapper;
using hamster.Configuration;
using hamster.DTO;
using hamster.Model;
using hamster.Services;
using hamster.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var configFilePath = PathUtils.ConfigFilePath();
if (!File.Exists(configFilePath))
{
    Console.WriteLine("Config file not found");
    return;
}

var configJson = File.ReadAllText(configFilePath);
var config = JsonSerializer.Deserialize<ConfigFile>(configJson);

if (config == null || args.Length < 1)
    return;

var operationName = args[0];
var operation = config.Operations.FirstOrDefault(f => f.Name.Equals(operationName, StringComparison.OrdinalIgnoreCase));
Guard.Against.Null(operation, nameof(operation));

var mapper = new MapperConfiguration(mc =>  mc.AddProfile(new AutoMapping()))
    .CreateMapper();

var configDto = mapper.Map<HamsterConfigDto>(config);
var operationToExecuteDto = mapper.Map<BackupOperationDto>(config.Operations[0]);

var serviceProvider = BuildServiceProvider();

var logger = serviceProvider.GetService<ILogger<Program>>();
logger?.LogInformation("Hamster started");


var operationExecutive = serviceProvider.GetService<OperationExecutive>();
var executeResult = await operationExecutive?.Execute(operationToExecuteDto)!;

if (executeResult)
{
    await NotifyUtils.SendNotification(operationName, "Backup success", "Backup complete successfully.", "information");
}
else
{
    await NotifyUtils.SendNotification(operationName, "Backup failed", "Failed to get backup.", "error");
}

logger?.LogCritical("Hamster done");


ServiceProvider BuildServiceProvider()
{
    return new ServiceCollection()
        .AddLogging(options =>
        {
            options.AddSimpleConsole(opt => 
            {
                opt.SingleLine = true;
                opt.TimestampFormat = "MM/dd/yyyy hh:mm:ss => ";
            });
        })
        .AddSingleton<OperationExecutive>()
        .AddScoped<AmazonS3ObjectStorage>()
        .AddScoped<UploadFileUtils>()
        .AddScoped<CompressUtils>()
        .AddScoped<NotifyUtils>()
        .AddSingleton(mapper)
        .AddSingleton(configDto)
        .AddAutoMapper(typeof(Program))
        .BuildServiceProvider();
}