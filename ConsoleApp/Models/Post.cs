using System.Text.Json.Serialization;

namespace ConsoleApp.Models;

public class Post
{
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("body")]
    public string Body { get; set; }
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    public override string ToString()
    {
        return $"Post #{Id} (User: {UserId})\n" +
               $"  Title: {Title}\n" +
               $"  Body: {Body.Substring(0, Math.Min(50, Body.Length))}...";
    }
}