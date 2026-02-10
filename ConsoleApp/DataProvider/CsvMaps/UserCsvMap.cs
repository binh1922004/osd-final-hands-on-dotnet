using ConsoleApp.Models;
using CsvHelper.Configuration;

namespace ConsoleApp.DataProvider.CsvMaps;

public class UserCsvMap : ClassMap<User>
{
    public UserCsvMap()
    {
        Map(m => m.Id).Name("id");
        Map(m => m.Name).Name("name");
        Map(m => m.Username).Name("username");
        Map(m => m.Email).Name("email");
        Map(m => m.Phone).Name("phone");
        Map(m => m.Website).Name("website");
        
        // Map nested Address object from pipe-delimited string
        Map(m => m.Address).Convert(args =>
        {
            var addressString = args.Row.GetField("address");
            if (string.IsNullOrEmpty(addressString)) return null;
            
            var parts = addressString.Split('|');
            if (parts.Length < 4) return null;
            
            return new Address
            {
                Street = parts[0],
                Suite = parts[1],
                City = parts[2],
                Zipcode = parts[3]
            };
        });
        
        // Map nested Company object from pipe-delimited string
        Map(m => m.Company).Convert(args =>
        {
            var companyString = args.Row.GetField("company");
            if (string.IsNullOrEmpty(companyString)) return null;
            
            var parts = companyString.Split('|');
            if (parts.Length < 3) return null;
            
            return new Company
            {
                Name = parts[0],
                CatchPhrase = parts[1],
                Bs = parts[2]
            };
        });
    }
}
