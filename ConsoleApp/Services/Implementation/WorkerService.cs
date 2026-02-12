using ConsoleApp.Models;
using ConsoleApp.Services.Interface;
using ConsoleApp.Setting;
using ConsoleApp.Utilities;
using Microsoft.Extensions.Options;
using Serilog;

namespace ConsoleApp.Services.Implementation;

public class WorkerService(
    IOptions<CsvStorageSetting> csvSetting,
    IOptions<JsonStorageSetting> jsonSetting,
    IOptions<ExcelStorageSetting> excelSetting,
    IOptions<ApiSetting> apiSetting,
    IDataProcessService dataProcessService)
{
    private readonly ApiSetting _apiSetting = apiSetting.Value;
    private readonly CsvStorageSetting _csvSetting = csvSetting.Value;
    private readonly ExcelStorageSetting _excelSetting = excelSetting.Value;
    private readonly JsonStorageSetting _jsonSetting = jsonSetting.Value;

    public async Task DoJob(CancellationToken cancellationToken = default)
    {
        var csvTask = dataProcessService.GetCollectionDataAsync<User>(DataProviderType.Csv, _csvSetting.UserFilePath, cancellationToken);
        var jsonTask = dataProcessService.GetCollectionDataAsync<User>(DataProviderType.Json, _jsonSetting.UserFilePath, cancellationToken);
        var excelTask = dataProcessService.GetCollectionDataAsync<User>(DataProviderType.Excel, _excelSetting.UserFilePath, cancellationToken);
        var apiTask = dataProcessService.GetCollectionDataAsync<User>(DataProviderType.Api, _apiSetting.UserApiUrl, cancellationToken);
        
        // Wait for all tasks to complete and get results directly
        var results = await Task.WhenAll(csvTask, jsonTask, excelTask, apiTask);
        
        // Combine all results into a single list
        var combinedList = new List<User>();
        foreach (var result in results)
        {
            combinedList.AddRange(result);
        }
        var summaryList = combinedList.Select(u => new 
        {
            u.Id,
            u.Name,
            u.Username
        }).ToList();
        
        Log.Information("Combined results from all sources - Total: {TotalCount} records", combinedList.Count);
        await dataProcessService.WriteDataToCsvFile(summaryList, "/Users/mac/Orient/Project/ConsoleApp/ConsoleApp/Storage/ResultData.csv", cancellationToken);
    }
}