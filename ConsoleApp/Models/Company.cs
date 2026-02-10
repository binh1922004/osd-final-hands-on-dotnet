using System.Text.Json.Serialization;

namespace ConsoleApp.Models;

public class Company
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("catchPhrase")] 
    public string CatchPhrase { get; set; }
    [JsonPropertyName("bs")]
    public string Bs { get; set; }
}