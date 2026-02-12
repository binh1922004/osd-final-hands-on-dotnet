namespace ConsoleApp.DataProvider.Interface;

public interface ICollectionDataProvider
{
    Task<IEnumerable<T>> GetCollectionDataAsync<T>(string filePath, CancellationToken cancellationToken)
        where T : class, new();
}