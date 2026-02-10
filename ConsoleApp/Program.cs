using ConsoleApp.DataProvider.Implementation;
using ConsoleApp.DataProvider.Interface;
using ConsoleApp.Logger;
using ConsoleApp.Models;
using ConsoleApp.Services.Implementation;
using ConsoleApp.Services.Interface;
using ConsoleApp.Setting;
using ConsoleApp.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
// using ConsoleApp.Data.Implementation;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .Build();
var logFilePath = configuration.GetSection("LogFilePath").Value ?? "app.log";
var logFileWriter = new StreamWriter(logFilePath, append: true)
{
    AutoFlush = true
};
var services = new ServiceCollection();

// Add configuration to DI
services.AddSingleton<IConfiguration>(configuration);

// Configure Options Pattern - Bind settings from appsettings.json
services.Configure<CsvStorageSetting>(configuration.GetSection("CsvStorageSetting"));
services.Configure<JsonStorageSetting>(configuration.GetSection("JsonStorageSetting"));
services.Configure<ApiSetting>(configuration.GetSection("ApiSetting"));
services.AddScoped<IUserService, UserService>();
services.AddScoped<IPostService, PostService>();
services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.SetMinimumLevel(LogLevel.Information);
    loggingBuilder.AddProvider(new CustomFileLoggerProvider(logFileWriter));
});
services.AddKeyedScoped<IDataProvider, JsonDataProvider>("Json");
services.AddKeyedScoped<IDataProvider, CsvDataProvider>("Csv");
services.AddKeyedScoped<IDataProvider, ApiDataProvider>("Api");
services.AddTransient<HttpClient>();
services.AddScoped<IDataProcessService, DataProcessService>();
services.AddScoped<WorkerService>();
var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>(); 
var workerService = serviceProvider.GetRequiredService<WorkerService>();

while (true)
{
    try
    {
        logger.LogInformation("Starting console application...");
        await workerService.DoJob();
        logger.LogInformation("Console application finished successfully.");
    }
    catch (Exception ex) 
    {
        logger.LogError(ex, "An error occurred during processing.");

    }
    Console.WriteLine("Do you want to try again? (Y/N)");
    var retryInput = Console.ReadLine();
    if (!string.Equals(retryInput, "Y", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }
}