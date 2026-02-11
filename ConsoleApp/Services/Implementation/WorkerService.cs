using ConsoleApp.Models;
using ConsoleApp.Services.Interface;
using ConsoleApp.Setting;
using ConsoleApp.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsoleApp.Services.Implementation;

public class WorkerService(
    IOptions<CsvStorageSetting> csvSetting,
    IOptions<JsonStorageSetting> jsonSetting,
    IOptions<ExcelStorageSetting> excelSetting,
    IOptions<ApiSetting> apiSetting,
    ILogger<WorkerService> logger,
    IUserService userService,
    IPostService postService,
    IDataProcessService dataProcessService)
{
    private readonly CsvStorageSetting _csvSetting = csvSetting.Value;
    private readonly JsonStorageSetting _jsonSetting = jsonSetting.Value;
    private readonly ExcelStorageSetting _excelSetting = excelSetting.Value;
    private readonly ApiSetting _apiSetting = apiSetting.Value;

    public async Task DoJob()
    {
        // Define data collection sources based on provider type and collection choice
        // 1 for User data and 2 for Post data
        Dictionary<DataProviderType, Dictionary<int, string>> dataCollectionSources = new()
        {
            {
                DataProviderType.Json, new Dictionary<int, string>
                {
                    {1, _jsonSetting.UserFilePath},
                    {2, _jsonSetting.PostFilePath}
                }
            },
            {
                DataProviderType.Csv, new Dictionary<int, string>
                {
                    {1, _csvSetting.UserFilePath},
                    {2, _csvSetting.PostFilePath}
                }
            },
            {
                DataProviderType.Excel, new Dictionary<int, string>
                {
                    {1, _excelSetting.UserFilePath},
                    {2, _excelSetting.PostFilePath}
                }
            },
            {
                DataProviderType.Api, new Dictionary<int, string>
                {
                    {1, _apiSetting.UserApiUrl},
                    {2, _apiSetting.PostApiUrl}
                }
            }
        };
        while (true)
        {
            Console.WriteLine("Select Data Provider Type (Json, Csv, Api, Excel) or 'Exit' to quit:");
            var input = Console.ReadLine();
            if (string.Equals(input, "Exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (!Enum.TryParse<DataProviderType>(input, true, out var dataProviderType))
            {
                Console.WriteLine("Invalid input. Please enter 'Json', 'Csv', 'Api', 'Excel' or 'Exit'.");
                continue;
            }
            Console.WriteLine("What data collection do you want to process: 1. User - 2. Post");
            var collectionInput = Console.ReadLine();
        
            if (!int.TryParse(collectionInput, out var collectionChoice) || (collectionChoice != 1 && collectionChoice != 2))
            {
                Console.WriteLine("Invalid input. Please enter '1' for User or '2' for Post.");
                continue;
            }
            
            var source = dataCollectionSources[dataProviderType][collectionChoice];
            try
            {
                logger.LogInformation("Starting getting data");
                
                if (dataProviderType is DataProviderType.Json or DataProviderType.Api)
                {
                    if (collectionChoice == 1)
                    {
                        var users = await dataProcessService.GetDataAsync<List<User>>(dataProviderType, source);
                        userService.InteractWithUsers(users);
                    }
                    else
                    {
                        var posts = await dataProcessService.GetDataAsync<List<Post>>(dataProviderType, source);
                        postService.InteractWithPosts(posts);
                    }
                }
                else  // Csv case
                {
                    if (collectionChoice == 1)
                    {
                        var users = await dataProcessService.GetCollectionDataAsync<User>(dataProviderType, source);
                        userService.InteractWithUsers(users.ToList());
                    }
                    else
                    {
                        var posts = await dataProcessService.GetCollectionDataAsync<Post>(dataProviderType, source);
                        postService.InteractWithPosts(posts.ToList());
                    }
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred during data processing.");
                throw;
            }
        }
    }
}