using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using BlazorApp.Shared;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace BlazorApp.Api
{
    public static class WeatherForecastFunction
    {
        private static string GetSummary(int temp)
        {
            var summary = "Mild";

            if (temp >= 32)
            {
                summary = "Hot";
            }
            else if (temp <= 16 && temp > 0)
            {
                summary = "Cold";
            }
            else if (temp <= 0)
            {
                summary = "Freezing";
            }

            return summary;
        }

//   [FunctionName("WeatherForecastGet")]
//         public static IActionResult Run(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
//             ILogger log)
//         {
//             var s = new WeatherForecast();
//             return new OkObjectResult(s);

//         }

        [FunctionName("WeatherForecast")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            CloudStorageAccount storageAccount =
        new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials("algebraiot", "6GY4q2/HYcVmpGMrNDpHuKB5c9O3CWyPCv9l/JhDD/N9AmWrq92jOv0eUeYysX34/Gfrj3RQa+VPoOglCFs4gw=="),
        true);
    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

    CloudTable _linkTable = tableClient.GetTableReference("tempHumLog");
    await _linkTable.CreateIfNotExistsAsync();

    // Create a new customer entity.
     var data = await req.Content.ReadAsAsync<WeatherForecastDTO>();

    data.PartitionKey = "partition1";
    data.RowKey = Guid.NewGuid().ToString();
    // Create the TableOperation that inserts the customer entity.
    TableOperation insertOperation = TableOperation.InsertOrMerge(data);
          
    var bSuccess = true;
           try
    {

    await _linkTable.ExecuteAsync(insertOperation);

    }
    catch (Exception e)
    {
        bSuccess = false;
        return new OkObjectResult(data);
    }
     ;

            return new OkObjectResult(bSuccess);
        }
    }

    
}
