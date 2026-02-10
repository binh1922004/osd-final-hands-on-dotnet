using ConsoleApp.DataProvider.Interface;
using ConsoleApp.Models;
using ConsoleApp.Services.Interface;
using ConsoleApp.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.Services.Implementation;

public class DataProcessService(
    ILogger<DataProcessService> logger,
    [FromKeyedServices("Json")] IDataProvider jsonProvider,
    [FromKeyedServices("Csv")] IDataProvider csvProvider,
    [FromKeyedServices("Api")] IDataProvider apiProvider) : IDataProcessService
{
    public async Task<T> GetDataAsync<T>(DataProviderType dataProviderType, string source)
    {
        logger.LogInformation("Getting posts for {dataProviderType} - ", dataProviderType);
        var dataProvider = dataProviderType switch
        {
            DataProviderType.Json => jsonProvider,
            DataProviderType.Csv => csvProvider,
            DataProviderType.Api => apiProvider,
            _ => throw new ArgumentOutOfRangeException(nameof(dataProviderType), $"Unsupported data provider type: {dataProviderType}")
        };

        return await dataProvider.GetDataAsync<T>(source);
    }

}