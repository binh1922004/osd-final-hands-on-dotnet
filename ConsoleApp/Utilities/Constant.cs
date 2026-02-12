namespace ConsoleApp.Utilities;

public class Constant
{
    // Configuration Keys
    public const string LogFilePathKey = "LogFilePath";
    public const string CsvStorageSettingKey = "CsvStorageSetting";
    public const string JsonStorageSettingKey = "JsonStorageSetting";
    public const string ExcelStorageSettingKey = "ExcelStorageSetting";
    public const string ApiSettingKey = "ApiSetting";
    
    // Configuration Files
    public const string AppSettingsFileName = "appSettings.json";
    
    // Default Values
    public const string DefaultLogFileName = "app.log";
    
    // Data Provider Keys
    public const string JsonProviderKey = "Json";
    public const string CsvProviderKey = "Csv";
    public const string ExcelProviderKey = "Excel";
    public const string ApiProviderKey = "Api";
    
    // User Prompts
    public const string RetryPrompt = "Do you want to try again? (Y/N)";
    public const string YesInput = "Y";
    
    // Data Provider Names for Logging
    public const string JsonProviderName = "JSON";
    public const string CsvProviderName = "CSV";
    public const string ExcelProviderName = "Excel";
    public const string ApiProviderName = "API";
}