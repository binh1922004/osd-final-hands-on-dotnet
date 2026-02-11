namespace ConsoleApp.DataProvider.Interface;

public interface ICollectionDataProvider
{
    Task<IEnumerable<T>> GetCollectionDataAsync<T>(string source) where T : class, new();
}