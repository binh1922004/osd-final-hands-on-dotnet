using ConsoleApp.Models;
using ConsoleApp.Services.Interface;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.Services.Implementation;

public class UserService(ILogger<UserService> logger) : IUserService
{
    public List<User> GetUsersByCompanyName(List<User> users, string companyName)
    {
        logger.LogInformation("Starting getting users by company name: {CompanyName}", companyName);
        var usersResult = users.Where(u => u.Company.Name.ToLower().Contains(companyName.ToLower())).ToList();
        logger.LogInformation("Finished getting users by company name: {CompanyName}", companyName);
        return usersResult;
    }

    public List<User> FilterUsersByCity(List<User> users, string city)
    {
        logger.LogInformation("Starting filtering users by City: {City}", city);
        var usersResult = users.Where(u => u.Address.City.ToLower().Contains(city.ToLower())).ToList();
        logger.LogInformation("Finished filtering users by City: {City}", city);
        return usersResult;
    }

    public void InteractWithUsers(List<User> users)
    {
        while (true)
        {
            Console.WriteLine("Select your option: 1. Get Users by Company Name, 2. Filter Users by City, q. Exit");
            var input = Console.ReadLine();
            if (input == "q") break;

            switch (input)
            {
                case "1":
                {
                    Console.WriteLine("Enter Company Name to search:");
                    var companyName = Console.ReadLine();
                    if (string.IsNullOrEmpty(companyName))
                    {
                        Console.WriteLine("Company Name cannot be empty. Please try again.");
                        continue;
                    }

                    var resultUsers = GetUsersByCompanyName(users, companyName);
                    Console.WriteLine($"Found {resultUsers.Count} users for Company Name containing '{companyName}':");
                    foreach (var user in resultUsers) Console.WriteLine($"- {user.Name} ({user.Company.Name})");
                    break;
                }
                case "2":
                {
                    Console.WriteLine("Enter City to filter:");
                    var city = Console.ReadLine();
                    if (string.IsNullOrEmpty(city))
                    {
                        Console.WriteLine("City cannot be empty. Please try again.");
                        continue;
                    }

                    var resultUsers = FilterUsersByCity(users, city);
                    Console.WriteLine($"Found {resultUsers.Count} users in City containing '{city}':");
                    foreach (var user in resultUsers) Console.WriteLine($"- {user.Name} ({user.Address.City})");
                    break;
                }
                default:
                {
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
                }
            }
        }
    }
}