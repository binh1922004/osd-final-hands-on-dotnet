using System.Collections.Concurrent;
using System.Text.Json;
using ConsoleApp.DataProvider.Interface;
using ConsoleApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.DataProvider.Implementation;

public class JsonDataProvider(
    ILogger<JsonDataProvider> logger,
    IConfiguration configuration) : IDataProvider
{
    

    public async Task<T> GetDataAsync<T>(string source)
    {
        try
        {
            logger.LogInformation("Start reading product data from JSON file: {FileName}", source);

            if (!File.Exists(source))
            {
                logger.LogWarning("JSON file not found: {FileName}", source);
                throw new FileNotFoundException("JSON file not found", source);
            }

            var jsonContent = await File.ReadAllTextAsync(source);
            logger.LogInformation("Finished reading product data from JSON file: {FileName}", source);
            var jsonData = JsonSerializer.Deserialize<T>(jsonContent);

            logger.LogInformation("Successfully loaded posts from JSON file");
            return jsonData;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize JSON file: {FileName}", source);
            throw;
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Failed to read JSON file: {FileName}", source);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while reading product data from JSON file: {FileName}", source);
            throw;
        }
    }
}