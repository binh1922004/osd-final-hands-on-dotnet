using ConsoleApp.DataProvider.Implementation;
using ConsoleApp.DataProvider.Interface;
using ConsoleApp.Services.Implementation;
using ConsoleApp.Services.Interface;
using ConsoleApp.Setting;
using ConsoleApp.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using Serilog;


// Build configuration
// Use AppContext.BaseDirectory to get the directory where the application is running
// This works correctly even when running from bin/Debug/net10.0/
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile(Constant.AppSettingsFileName, false, true)
    .Build();


var logFilePath = configuration.GetSection(Constant.LogFilePathKey).Value ?? Constant.DefaultLogFileName;
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

var services = new ServiceCollection();

// Add configuration to DI
services.AddSingleton<IConfiguration>(configuration);

// Configure Options Pattern - Bind settings from appSettings.json
services.Configure<CsvStorageSetting>(configuration.GetSection(Constant.CsvStorageSettingKey));
services.Configure<JsonStorageSetting>(configuration.GetSection(Constant.JsonStorageSettingKey));
services.Configure<ExcelStorageSetting>(configuration.GetSection(Constant.ExcelStorageSettingKey));
services.Configure<ApiSetting>(configuration.GetSection(Constant.ApiSettingKey));
services.AddKeyedScoped<ICollectionDataProvider, JsonDataProvider>(Constant.JsonProviderKey);
services.AddKeyedScoped<ICollectionDataProvider, CsvDataProvider>(Constant.CsvProviderKey);
services.AddKeyedScoped<ICollectionDataProvider, ExcelDataProvider>(Constant.ExcelProviderKey);
services.AddKeyedScoped<ICollectionDataProvider, ApiDataProvider>(Constant.ApiProviderKey);
services.AddScoped<IDataProcessService, DataProcessService>();
services.AddScoped<WorkerService>();
services.AddHttpClient();

ExcelPackage.License.SetNonCommercialPersonal("ball");
var serviceProvider = services.BuildServiceProvider();
var workerService = serviceProvider.GetRequiredService<WorkerService>();

try
{
    Log.Information("Starting console application...");
    
    // Get timeout from configuration (default to 200ms if not specified)
    // var timeoutMs = configuration.GetValue<int>(Constant.RequestTimeoutMsKey);
    // if (timeoutMs <= 0) timeoutMs = 200;
    //
    // using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
    await workerService.DoJob();
    
    Log.Information("Console application finished successfully.");
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred during processing.");
}
