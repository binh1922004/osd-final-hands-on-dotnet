// using ConsoleApp.DataProvider.Interface;
// using ConsoleApp.Models;
// using ConsoleApp.Utilities;
// using MiniExcelLibs;
// using MiniExcelLibs.OpenXml;
// using Serilog;
//
// namespace ConsoleApp.DataProvider.Implementation;
//
// public class ExcelDataProvider : ICollectionDataProvider
// {
//     public async Task<IEnumerable<T>> GetCollectionDataAsync<T>(string source, CancellationToken cancellationToken)
//         where T : class, new()
//     {
//         Log.Information("Start reading data from Excel source: {Source}", source);
//         if (!File.Exists(source))
//         {
//             Log.Warning("Excel file not found: {Source}", source);
//             throw new FileNotFoundException("Excel file not found", source);
//         }
//
//         List<T> rows = [];
//         var rowNumber = 0;
//         var rowsa = MiniExcel.Query<T>(source, hasHeader: false);
//         // await foreach (var row in MiniExcel.QueryAsync<T>(source, hasHeader: false,
//         //                    cancellationToken: cancellationToken))
//         // {
//         //     try
//         //     {
//         //         rowNumber++;
//         //         if (row is not IParseable parseable)
//         //         {
//         //             var error = new ErrorRow
//         //             {
//         //                 Type = DataProviderType.Excel,
//         //                 RowNumber = rowNumber,
//         //                 ErrorMessage = "Row does not implement IParseable"
//         //             };
//         //             Log.Error(error.ToString());
//         //             continue;
//         //         }
//         //
//         //         if (!parseable.ParseRawFields())
//         //         {
//         //             var error = new ErrorRow
//         //             {
//         //                 Type = DataProviderType.Excel,
//         //                 RowNumber = rowNumber,
//         //                 ErrorMessage = "Failed to parse raw fields"
//         //             };
//         //             Log.Error(error.ToString());
//         //             continue;
//         //         }
//         //
//         //         rows.Add(row);
//         //     }
//         //     catch (Exception ex)
//         //     {
//         //         var error = new ErrorRow
//         //         {
//         //             Type = DataProviderType.Excel,
//         //             RowNumber = rowNumber,
//         //             ErrorMessage = ex.Message
//         //         };
//         //         Log.Error(error.ToString());
//         //     }
//         // }
//
//         Log.Information("Successfully loaded {ValidCount} records from Excel source: {Source}", rows.Count, source);
//         return rows;
//     }
// }


using ConsoleApp.DataProvider.Interface;
using ConsoleApp.Models;
using ConsoleApp.Utilities;
using OfficeOpenXml;
using Serilog;
using System.Reflection;
using MiniExcelLibs.Attributes;

namespace ConsoleApp.DataProvider.Implementation;

public class ExcelDataProvider : ICollectionDataProvider
{
    public async Task<IEnumerable<T>> GetCollectionDataAsync<T>(string filePath, CancellationToken cancellationToken)
        where T : class, new()
    {
        Log.Information("Start reading data from {Provider} source: {Source}", Constant.ExcelProviderName, filePath);
        
        if (!File.Exists(filePath))
        {
            Log.Warning("{Provider} file not found: {Source}", Constant.ExcelProviderName, filePath);
            throw new FileNotFoundException("Excel file not found", filePath);
        }

        // Set EPPlus license context

        List<T> rows = [];
        var skippedCount = 0;

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0]; // First sheet
            
            if (worksheet.Dimension == null)
            {
                Log.Warning("{Provider} worksheet is empty: {Source}", Constant.ExcelProviderName, filePath);
                return rows;
            }

            var startRow = worksheet.Dimension.Start.Row;
            var endRow = worksheet.Dimension.End.Row;
            var startCol = worksheet.Dimension.Start.Column;
            var endCol = worksheet.Dimension.End.Column;

            // Get column mapping from header row (row 1)
            var columnMapping = new Dictionary<string, int>();
            for (var col = startCol; col <= endCol; col++)
            {
                var headerValue = worksheet.Cells[startRow, col].Text;
                if (!string.IsNullOrWhiteSpace(headerValue))
                {
                    columnMapping[headerValue.Trim()] = col;
                }
            }

            // Get property mapping with ExcelColumnName attribute
            var properties = typeof(T).GetProperties();
            var propertyMapping = new Dictionary<PropertyInfo, int>();
            
            // Map properties to columns based on ExcelColumnName attribute or property name
            foreach (var prop in properties)
            {
                var excelAttr = prop.GetCustomAttribute<ExcelColumnNameAttribute>();
                var columnName = excelAttr?.ExcelColumnName ?? prop.Name;
                
                if (columnMapping.TryGetValue(columnName, out var colIndex))
                {
                    propertyMapping[prop] = colIndex;
                }
            }

            // Read data rows (skip header)
            for (int row = startRow + 1; row <= endRow; row++)
            {
                try
                {
                    var instance = new T();
                    bool hasValue = false, rowHasError = false;

                    foreach (var (prop, colIndex) in propertyMapping)
                    {
                        if (rowHasError)
                        {
                            break;
                        }
                        try
                        {
                            var cellValue = worksheet.Cells[row, colIndex].Text;
                            
                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                hasValue = true;
                                
                                // Convert value to property type
                                var convertedValue = Convert.ChangeType(cellValue, 
                                    Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                                prop.SetValue(instance, convertedValue);
                            }
                        }
                        catch (Exception ex)
                        {
                            var error = new ErrorRow
                            {
                                Type = DataProviderType.Excel,
                                RowNumber = row,
                                ErrorMessage = $"Failed to set property {prop.Name}: {ex.Message}"
                            };
                            Log.Error(error.ToString());
                            skippedCount++;
                            rowHasError = true;
                        }
                    }

                    if (!hasValue)
                    {
                        skippedCount++;
                        continue; // Skip empty rows
                    }

                    if (rowHasError)
                    {
                        continue;
                    }

                    // Validate with IParseable if applicable
                    if (instance is IParseable parseable)
                    {
                        if (!parseable.ParseRawFields())
                        {
                            var error = new ErrorRow
                            {
                                Type = DataProviderType.Excel,
                                RowNumber = row,
                                ErrorMessage = "Failed to parse raw fields"
                            };
                            Log.Error(error.ToString());
                            skippedCount++;
                            continue;
                        }
                    }

                    rows.Add(instance);
                }
                catch (Exception ex)
                {
                    var error = new ErrorRow
                    {
                        Type = DataProviderType.Excel,
                        RowNumber = row,
                        ErrorMessage = ex.Message
                    };
                    Log.Error(error.ToString());
                    skippedCount++;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("Failed to read {Provider} file {Source}: {Message}", Constant.ExcelProviderName, filePath, ex.Message);
            throw;
        }

        Log.Information("Successfully loaded {ValidCount} records from {Provider} source: {Source} (Skipped: {SkippedCount})", 
            rows.Count, Constant.ExcelProviderName, filePath, skippedCount);
        
        return rows;
    }
}