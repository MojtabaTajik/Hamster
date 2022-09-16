
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

try
{
    var configJson = File.ReadAllText(configFilePath);
    var config = JsonSerializer.Deserialize<ConfigFile>(configJson);

    Guard.Against.Null(config, nameof(config));
    Guard.Against.Zero(args.Length, nameof(args.Length), "Operation name is mandatory.");

    var operationName = args[0];
    var operation =
        config.Operations.FirstOrDefault(f => f.Name.Equals(operationName, StringComparison.OrdinalIgnoreCase));
    
    Guard.Against.Null(operation, nameof(operation));
    
    var serviceProvider = BuildServiceProvider(config, operation);
    
    var logger = serviceProvider.GetService<ILogger<Program>>();
    logger?.LogInformation("Hamster started");
    
    var operationExecutive = serviceProvider.GetService<OperationExecutive>();
    var executeResult = await operationExecutive?.Execute()!;

    if (executeResult)
    {
        await NotifyUtils.SendNotification(operationName, "Backup success", "Backup complete successfully.",
            "information");
    }
    else
    {
        await NotifyUtils.SendNotification(operationName, "Backup failed", "Failed to get backup.", "error");
    }

    logger?.LogCritical("Hamster done");
}
catch (Exception ex)
{

}


ServiceProvider BuildServiceProvider(ConfigFile config, BackupOperation operation)
{
    var mapper = new MapperConfiguration(mc =>
        mc.AddProfile(new AutoMapping())).CreateMapper();
    
    var configDto = mapper.Map<HamsterConfigDto>(config);
    var operationToExecuteDto = mapper.Map<BackupOperationDto>(operation);
    
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
        .AddSingleton(operationToExecuteDto)
        .AddAutoMapper(typeof(Program))
        .BuildServiceProvider();
}