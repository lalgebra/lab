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
using System.Collections.Generic;

namespace BlazorApp.Api
{
    public static class WeatherForecastFunction
    {
        public static string account = "algebraiot";
        public static string tableName = "tempHumLog";
        public static string partitionKey = "temperatureAndHumidity";
        public static string key = "6GY4q2/HYcVmpGMrNDpHuKB5c9O3CWyPCv9l/JhDD/N9AmWrq92jOv0eUeYysX34/Gfrj3RQa+VPoOglCFs4gw==";


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

        [FunctionName("WeatherForecastGet")]
        public static async Task<IActionResult> Run(
                  [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getAll")] HttpRequest req,
                  ILogger log)
        {
            
            var _records = new List<WeatherForecast>();
            var storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(account, key), true);
            var tableClient = storageAccount.CreateCloudTableClient();

            var _linkTable = tableClient.GetTableReference(tableName);

            await _linkTable.CreateIfNotExistsAsync();

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            var query = new TableQuery<WeatherForecast>();

            // Print the fields for each customer.
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<WeatherForecast> resultSegment = await _linkTable.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;

                foreach (var entity in resultSegment.Results)
                {
                    WeatherForecast _summary = new WeatherForecast
                    {
                        PartitionKey = entity.PartitionKey,
                        RowKey = entity.RowKey,
                        Timestamp = entity.Timestamp
                    };

                    _records.Add(_summary);
                }
            } while (token != null);

            return new OkObjectResult(_records);

        }

        [FunctionName("WeatherForecast")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            CloudStorageAccount storageAccount =
        new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(account, key),
        true);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable _linkTable = tableClient.GetTableReference(tableName);
            await _linkTable.CreateIfNotExistsAsync();

            // Create a new customer entity.
            var data = await req.Content.ReadAsAsync<WeatherForecast>();

            data.PartitionKey = partitionKey;
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

            };

            return new OkObjectResult(bSuccess);
        }
    }


}
