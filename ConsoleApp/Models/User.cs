using System.Text.Json.Serialization;
using ConsoleApp.DataProvider.Interface;
using CsvHelper.Configuration.Attributes;
using MiniExcelLibs.Attributes;

namespace ConsoleApp.Models;

public class User : IParseable
{
    [JsonPropertyName("id")]
    [ExcelColumnName("id")]
    [Name("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    [ExcelColumnName("name")]
    [Name("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("username")]
    [ExcelColumnName("username")]
    [Name("username")]
    public string Username { get; set; }
    
    [JsonPropertyName("email")]
    [ExcelColumnName("email")]
    [Name("email")]
    public string Email { get; set; }
    
    [ExcelColumnName("address")]
    [Name("address")]
    public string AddressRaw { get; set; }

    [JsonPropertyName("address")]
    [Ignore] // Ignore for CSV - will be populated by ParseRawFields()
    public Address Address { get; set; }
    
    [JsonPropertyName("phone")]
    [ExcelColumnName("phone")]
    [Name("phone")]
    public string Phone { get; set; }
    
    [JsonPropertyName("website")]
    [ExcelColumnName("website")]
    [Name("website")]
    public string Website { get; set; }
    
    [ExcelColumnName("company")]
    [Name("company")]
    public string CompanyRaw { get; set; }
    
    [JsonPropertyName("company")]
    [Ignore] // Ignore for CSV - will be populated by ParseRawFields()
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
    
    public void ParseRawFields()
    {
        // Parse AddressRaw
        if (!string.IsNullOrEmpty(AddressRaw))
        {
            var parts = AddressRaw.Split('|');
            if (parts.Length >= 4)
            {
                Address = new Address
                {
                    Street = parts[0],
                    Suite = parts[1],
                    City = parts[2],
                    Zipcode = parts[3]
                };
            }
        }

        // Parse CompanyRaw
        if (!string.IsNullOrEmpty(CompanyRaw))
        {
            var parts = CompanyRaw.Split('|');
            if (parts.Length >= 3)
            {
                Company = new Company
                {
                    Name = parts[0],
                    CatchPhrase = parts[1],
                    Bs = parts[2]
                };
            }
        }
    }
}