using System.Globalization;
using ConsoleApp.DataProvider.Interface;
using ConsoleApp.Models;
using ConsoleApp.Utilities;
using CsvHelper;
using CsvHelper.Configuration;
using Serilog;

namespace ConsoleApp.DataProvider.Implementation;

public class CsvDataProvider : ICollectionDataProvider
{
    public async Task<IEnumerable<T>> GetCollectionDataAsync<T>(string filePath, CancellationToken cancellationToken)
        where T : class, new()
    {
        try
        {
            Log.Information("Start reading data from {Provider} source: {Source}", Constant.CsvProviderName, filePath);

            if (!File.Exists(filePath))
            {
                Log.Warning("{Provider} file not found: {Source}", Constant.CsvProviderName, filePath);
                throw new FileNotFoundException("CSV file not found", filePath);
            }

            using var reader = new StreamReader(filePath);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                HeaderValidated = null, // Don't validate headers
                MissingFieldFound = null, // Ignore missing fields
                ReadingExceptionOccurred = args =>
                {
                    var error = new ErrorRow
                    {
                        Type = DataProviderType.Csv,
                        RowNumber = args.Exception.Context.Parser.Row,
                        ErrorMessage = args.Exception.Message
                    };
                    Log.Error(error.ToString());

                    return false;
                }
            };

            var result = new List<T>();
            using var csv = new CsvReader(reader, config);

            await foreach (var row in csv.GetRecordsAsync<T>(cancellationToken))
            {
                // If the model implements IParseable, parse raw fields
                if (row is IParseable parseable) parseable.ParseRawFields();
                result.Add(row);
            }

            Log.Information("Successfully loaded {ValidCount} records from {Provider} source: {Source}",
                result.Count, Constant.CsvProviderName, filePath);

            return result;
        }
        catch (IOException ex)
        {
            Log.Error(ex, "Failed to read {Provider} source: {Source}", Constant.CsvProviderName, filePath);
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error while reading data from {Provider} source: {Source}", Constant.CsvProviderName, filePath);
            throw;
        }
    }
}