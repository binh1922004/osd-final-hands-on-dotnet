using ConsoleApp.Models;

namespace ConsoleApp.DataProvider.Interface;

public interface IDataProvider
{
    Task<T> GetDataAsync<T>(string source);
}