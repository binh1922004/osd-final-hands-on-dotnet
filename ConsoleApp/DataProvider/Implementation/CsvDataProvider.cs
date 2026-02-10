using System.Collections;
using System.Globalization;
using ConsoleApp.DataProvider.CsvMaps;
using ConsoleApp.DataProvider.Interface;
using ConsoleApp.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.DataProvider.Implementation;

public class CsvDataProvider(
    ILogger<CsvDataProvider> logger,
    IConfiguration configuration) : IDataProvider
{

    public async Task<T> GetDataAsync<T>(string source)
    {
        try
        {
            logger.LogInformation("Start reading product data from CSV file: {FileName}", source);

            if (!File.Exists(source))
            {
                logger.LogWarning("CSV file not found: {FileName}", source);
                throw new FileNotFoundException("CSV file not found", source);
            }

            // Get the element type if T is a List
            var type = typeof(T);
            Type elementType;
            
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // T is List<SomeType>, get SomeType
                elementType = type.GetGenericArguments()[0];
            }
            else
            {
                // Don't support
                logger.LogError("Must use List<> for read from CSV");
                throw  new NotSupportedException("Must use List<> for read from CSV");
            }

            using var reader = new StreamReader(source);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args =>
                {
                    // Upper first char of header from CSV
                    var newHeader = char.ToUpper(args.Header[0]) + args.Header[1..];
                    return newHeader;
                }
            };


            var result = (IList) Activator.CreateInstance<T>()!;
            
            using var csv = new CsvReader(reader, config);
            
            // Register custom ClassMap for User type to handle nested properties
            if (elementType == typeof(User))
            {
                csv.Context.RegisterClassMap<UserCsvMap>();
            }
            
            await foreach (var record in csv.GetRecordsAsync(elementType))
            {
                result.Add(record);    
            }

            return (T)result;
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