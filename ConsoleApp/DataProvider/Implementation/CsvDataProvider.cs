using System.Globalization;
using ConsoleApp.DataProvider.Interface;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.DataProvider.Implementation;

public class CsvDataProvider(ILogger<CsvDataProvider> logger) : ICollectionDataProvider
{
    public async Task<IEnumerable<T>> GetCollectionDataAsync<T>(string source) where T : class, new()
    {
        try
        {
            logger.LogInformation("Start reading product data from CSV file: {FileName}", source);

            if (!File.Exists(source))
            {
                logger.LogWarning("CSV file not found: {FileName}", source);
                throw new FileNotFoundException("CSV file not found", source);
            }

            using var reader = new StreamReader(source);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                HeaderValidated = null, // Don't validate headers
                MissingFieldFound = null // Ignore missing fields
            };

            var result = new List<T>();
            using var csv = new CsvReader(reader, config);
            
            await foreach (var row in csv.GetRecordsAsync<T>())
            {
                // If the model implements IParseable, parse raw fields
                if (row is IParseable parseable)
                {
                    parseable.ParseRawFields();
                }
                result.Add(row);
            }
            
            logger.LogInformation("Finished reading product data from CSV file with {Count} records: {FileName}", result.Count, source);

            return result;
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Failed to read CSV file: {FileName}", source);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while reading product data from CSV file: {FileName}", source);
            throw;
        }
    }
}