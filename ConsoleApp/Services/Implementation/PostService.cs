using ConsoleApp.Models;
using ConsoleApp.Services.Interface;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.Services.Implementation;

public class PostService(ILogger<PostService> logger) : IPostService
{
    public List<Post> GetPostsByUserId(List<Post> posts, int userId)
    {
        logger.LogInformation("Start getting posts for userId: {UserId}", userId);   
        var resultPosts = posts.Where(p => p.UserId == userId).ToList();
        logger.LogInformation("Finish getting posts for userId: {UserId}", userId);
        return resultPosts;
    }

    public List<Post> FilterPostsByKeyword(List<Post> posts, string keyword)
    {
        logger.LogInformation("Start filtering posts with keyword: {Keyword}", keyword);   
        var resultPosts = posts.Where(p => p.Title.ToLower().Contains(keyword.ToLower()) || 
                                           p.Body.ToLower().Contains(keyword.ToLower())).ToList();
        logger.LogInformation("Finish filtering posts with keyword: {Keyword}", keyword);
        return resultPosts;
    }

    public void InteractWithPosts(List<Post> posts)
    {
        while (true)
        {
            Console.WriteLine("Select your option: 1. Get Posts by User ID, 2. Filter Posts by Keyword, q. Exit");
            var input = Console.ReadLine();
            if (input == "q") break;

            switch (input)
            {
                case "1":
                {
                    Console.WriteLine("Enter User ID to search:");
                    var userIdInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(userIdInput))
                    {
                        Console.WriteLine("User ID cannot be empty. Please try again.");
                        continue;
                    }

                    if (!int.TryParse(userIdInput, out var userId))
                    {
                        Console.WriteLine("Invalid User ID. Please enter a valid number.");
                        continue;
                    }

                    var resultPosts = GetPostsByUserId(posts, userId);
                    Console.WriteLine($"Found {resultPosts.Count} posts for User ID '{userId}':");
                    foreach (var post in resultPosts) Console.WriteLine($"- {post.Title}");
                    break;
                }
                case "2":
                {
                    Console.WriteLine("Enter Keyword to filter:");
                    var keyword = Console.ReadLine();
                    if (string.IsNullOrEmpty(keyword))
                    {
                        Console.WriteLine("Keyword cannot be empty. Please try again.");
                        continue;
                    }

                    var resultPosts = FilterPostsByKeyword(posts, keyword);
                    Console.WriteLine($"Found {resultPosts.Count} posts containing keyword '{keyword}':");
                    foreach (var post in resultPosts) Console.WriteLine($"- {post.Title}");
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