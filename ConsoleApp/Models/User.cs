using System.Text.Json.Serialization;

namespace ConsoleApp.Models;

public class User
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("address")]
    public Address Address { get; set; }
    [JsonPropertyName("phone")]
    public string Phone { get; set; }
    [JsonPropertyName("website")]
    public string Website { get; set; }
    [JsonPropertyName("company")]
    public Company Company { get; set; }
    
    public override string ToString()
    {
        return $"User #{Id}: {Name} (@{Username})\n" +
               $"  Email: {Email}\n" +
               $"  Phone: {Phone}\n" +
               $"  Website: {Website}\n" +
               $"  Address: {Address?.City}, {Address?.Street}\n" +
               $"  Company: {Company?.Name}";
    }
}