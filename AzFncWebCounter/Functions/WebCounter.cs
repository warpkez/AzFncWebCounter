using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using AzFncWebCounter.Models;
using AzFncWebCounter.Methods;

namespace AzFncWebCounter.Functions
{
    public static class WebCounter
    {
        [FunctionName("WebCounter")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string dataBaseName = Environment.GetEnvironmentVariable("DatabaseName");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");
            string connectionString = Environment.GetEnvironmentVariable("ConnectionString");

            string json;

            try
            {
                CosmosClient cosmosClient = new(connectionString);

                await WebCounterMethod.CreateContainerIfNotExistsAsync(cosmosClient, dataBaseName, containerName);

                await WebCounterMethod.IsFirstTimeAsync(cosmosClient, dataBaseName, containerName);

                var counted = await WebCounterMethod.GetCountAsync(cosmosClient, dataBaseName, containerName);

                json = JsonConvert.SerializeObject(counted, Formatting.Indented);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);

                // At least return the error message in JSON format as part of the Id
                // This way, the client can still read the error message
                var error = new WebCounterModel
                {
                    Id = ex.Message,
                    Counter = -1,
                    Timestamp = DateTime.UtcNow
                };

                json = JsonConvert.SerializeObject(error);
            }

            // Even if everything breaks, it is okay! Won't they be surprised when they see the error message!
            return new OkObjectResult(json);
        }
    }
}
