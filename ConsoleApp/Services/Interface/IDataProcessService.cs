using ConsoleApp.Utilities;

namespace ConsoleApp.Services.Interface;

public interface IDataProcessService
{
    Task<IEnumerable<T>> GetCollectionDataAsync<T>(DataProviderType dataProviderType, string source, CancellationToken cancellationToken)
        where T : class, new();
    
    Task WriteDataToCsvFile<T>(IEnumerable<T> data, string filePath, CancellationToken cancellationToken)
        where T : class;
}