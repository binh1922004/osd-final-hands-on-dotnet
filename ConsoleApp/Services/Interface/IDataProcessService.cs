using ConsoleApp.Models;
using ConsoleApp.Utilities;

namespace ConsoleApp.Services.Interface;

public interface IDataProcessService
{
    
    Task<T> GetDataAsync<T>(DataProviderType dataProviderType, string source);
    
    Task<IEnumerable<T>> GetCollectionDataAsync<T>(DataProviderType dataProviderType, string source) where T : class, new();
}