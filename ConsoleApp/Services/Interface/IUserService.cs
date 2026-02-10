using ConsoleApp.Models;

namespace ConsoleApp.Services.Interface;

public interface IUserService
{
    List<User> GetUsersByCompanyName(List<User> users, string companyName);
    
    List<User> FilterUsersByCity(List<User> users, string city);
    
    void InteractWithUsers(List<User> users);
}