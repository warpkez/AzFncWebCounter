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

            string json;
            try
            {
                CosmosClient cosmosClient = new(Environment.GetEnvironmentVariable("DBConnStr"));

                await WebCounterMethod.CreateContainerIfNotExistsAsync(cosmosClient, Environment.GetEnvironmentVariable("DBName"), Environment.GetEnvironmentVariable("ContainerName"));

                await WebCounterMethod.IsFirstTimeAsync(cosmosClient, Environment.GetEnvironmentVariable("DBName"), Environment.GetEnvironmentVariable("ContainerName"));

                var counted = await WebCounterMethod.GetCountAsync(cosmosClient, Environment.GetEnvironmentVariable("DBName"), Environment.GetEnvironmentVariable("ContainerName"));

                json = JsonConvert.SerializeObject(counted, Formatting.Indented);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                json = JsonConvert.SerializeObject(ex.Message);
            }

            return new OkObjectResult(json);
        }
    }
}
