using System.Text.Json;
using ConsoleApp.DataProvider.Interface;
using ConsoleApp.Models;
using ConsoleApp.Utilities;
using Serilog;

namespace ConsoleApp.DataProvider.Implementation;

public class JsonDataProvider : ICollectionDataProvider
{
    public async Task<IEnumerable<T>> GetCollectionDataAsync<T>(string filePath, CancellationToken cancellationToken)
        where T : class, new()
    {
        try
        {
            Log.Information("Start reading data from {Provider} source: {Source}", Constant.JsonProviderName, filePath);
            if (!File.Exists(filePath))
            {
                Log.Warning("{Provider} file not found: {Source}", Constant.JsonProviderName, filePath);
                throw new FileNotFoundException("JSON file not found", filePath);
            }

            var jsonContent = await File.ReadAllTextAsync(filePath, cancellationToken);
            Log.Information("Successfully read content from {Provider} source: {Source}", Constant.JsonProviderName, filePath);
            using var jsonDocument = JsonDocument.Parse(jsonContent);
            var jsonArray = jsonDocument.RootElement;
            if (jsonArray.ValueKind != JsonValueKind.Array)
            {
                Log.Warning("{Provider} source does not contain an array: {Source}", Constant.JsonProviderName, filePath);
                throw new JsonException("JSON source does not contain an array");
            }
            
            var validItems = new List<T>();
            var skippedCount = 0;
            
            for (var i = 0; i < jsonArray.GetArrayLength(); i++)
            {
                try
                {
                    var element = jsonArray[i];
                    var item = JsonSerializer.Deserialize<T>(element.GetRawText());
                    if (item != null)
                    {
                        validItems.Add(item);
                    }
                    else
                    {
                        var error = new ErrorRow
                        {
                            Type = DataProviderType.Json,
                            RowNumber = i + 1,
                            ErrorMessage = "Deserialized item is null"
                        };
                        Log.Error(error.ToString());
                        skippedCount++;
                    }
                }
                catch (JsonException ex)
                {
                    var error = new ErrorRow
                    {
                        Type = DataProviderType.Json,
                        RowNumber = i + 1,
                        ErrorMessage = ex.Message
                    };
                    Log.Error(error.ToString());
                    skippedCount++;
                }
            }

            Log.Information("Successfully loaded {ValidCount} records from {Provider} source, skipped {SkippedCount} invalid records: {Source}", 
                validItems.Count, Constant.JsonProviderName, skippedCount, filePath);
            return validItems;
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to deserialize {Provider} source: {Source}", Constant.JsonProviderName, filePath);
            throw;
        }
        catch (IOException ex)
        {
            Log.Error(ex, "Failed to read {Provider} source: {Source}", Constant.JsonProviderName, filePath);
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error while reading data from {Provider} source: {Source}", Constant.JsonProviderName, filePath);
            throw;
        }
    }
    
}