using ConsoleApp.Utilities;

namespace ConsoleApp.Models;

public class ErrorRow
{
    public DataProviderType Type { get; init; }
    public int RowNumber { get; set; }
    public string ErrorMessage { get; set; }

    public override string ToString()
    {
        return $"[{Type}] Error at row {RowNumber}: {ErrorMessage}\n";
    }
}
//|21.8984