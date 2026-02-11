using System.Text.Json.Serialization;
using CsvHelper.Configuration.Attributes;
using MiniExcelLibs.Attributes;

namespace ConsoleApp.Models;

public class Post
{
    [JsonPropertyName("title")]
    [ExcelColumnName("title")]
    [Name("title")]
    public string Title { get; set; }
    [JsonPropertyName("body")]
    [ExcelColumnName("body")]
    [Name("body")]
    public string Body { get; set; }
    [JsonPropertyName("id")]
    [ExcelColumnName("id")]
    [Name("id")]
    public int Id { get; set; }
    [JsonPropertyName("userId")]
    [ExcelColumnName("userId")]
    [Name("userid")]
    public int UserId { get; set; }

    public override string ToString()
    {
        return $"Post #{Id} (User: {UserId})\n" +
               $"  Title: {Title}\n" +
               $"  Body: {Body.Substring(0, Math.Min(50, Body.Length))}...";
    }
}