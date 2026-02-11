using ConsoleApp.DataProvider.Interface;
using Microsoft.Extensions.Logging;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;

namespace ConsoleApp.DataProvider.Implementation;

public class ExcelDataProvider(ILogger<ExcelDataProvider> logger) : ICollectionDataProvider
{
    public async Task<IEnumerable<T>> GetCollectionDataAsync<T>(string source) where T : class, new()
    {
        logger.LogInformation("Reading Excel data from source: {source}", source);
        if (!File.Exists(source))
        {
            throw new FileNotFoundException("Excel file not found", source);
        }

        List<T> rows = [];
        await foreach(var row in MiniExcel.QueryAsync<T>(source, hasHeader:false))
        {
            // If the model implements IParseable, parse raw fields
            if (row is IParseable parseable)
            {
                parseable.ParseRawFields();
            }
            rows.Add(row);
        }
        logger.LogInformation("Finished reading product data from Excel file with {Count} records: {FileName}", rows.Count, source);
        return rows;
    }
}