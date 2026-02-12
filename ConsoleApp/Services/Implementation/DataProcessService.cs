using System.Diagnostics;
using System.Globalization;
using ConsoleApp.DataProvider.Interface;
using ConsoleApp.Services.Interface;
using ConsoleApp.Utilities;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ConsoleApp.Services.Implementation;

public class DataProcessService(
    [FromKeyedServices("Json")] ICollectionDataProvider jsonProvider,
    [FromKeyedServices("Csv")] ICollectionDataProvider csvProvider,
    [FromKeyedServices("Api")] ICollectionDataProvider apiProvider,
    [FromKeyedServices("Excel")] ICollectionDataProvider excelProvider) : IDataProcessService
{
    public async Task<IEnumerable<T>> GetCollectionDataAsync<T>(DataProviderType dataProviderType, string source, CancellationToken cancellationToken)
        where T : class, new()
    {
        var dataProvider = dataProviderType switch
        {
            DataProviderType.Csv => csvProvider,
            DataProviderType.Excel => excelProvider,
            DataProviderType.Json => jsonProvider,
            DataProviderType.Api => apiProvider,
            _ => throw new ArgumentOutOfRangeException(nameof(dataProviderType),
                $"Unsupported data provider type: {dataProviderType}")
        };
        var stopwatch = Stopwatch.StartNew();
        var result = await dataProvider.GetCollectionDataAsync<T>(source, cancellationToken);
        stopwatch.Stop();
        Log.Information("Data retrieval from {Provider} completed in {ElapsedMilliseconds}ms", 
            dataProviderType, stopwatch.ElapsedMilliseconds);
        return result;
    }

    public async Task WriteDataToCsvFile<T>(IEnumerable<T> data, string filePath, CancellationToken cancellationToken) where T : class
    {
        Log.Information("Writing data to file {FilePath}", filePath);
        await using var writer = new StreamWriter(filePath);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(data, cancellationToken);
        Log.Information("Data successfully written to file {FilePath}", filePath);
    }
}