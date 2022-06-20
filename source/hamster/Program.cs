using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using hamster.Model;
using hamster.Services;

const string configFileName = "config.json";
if (!File.Exists(configFileName))
    return;

var configContent = File.ReadAllText(configFileName);
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
    .AddSingleton(config)
    .BuildServiceProvider();

var logger = serviceProvider.GetService<ILogger<Program>>();
logger?.LogInformation("Hamster started");

var operationName = args[0];
var opExecuter = serviceProvider.GetService<OperationExecuter>();
var executeResult = opExecuter?.ExecuteOperation(operationName);

if (! string.IsNullOrEmpty(executeResult))
    logger?.LogInformation(executeResult);

Console.Read();