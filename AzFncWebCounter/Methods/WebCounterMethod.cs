using System;
using System.Linq;
using System.Threading.Tasks;
using AzFncWebCounter.Models;
using Microsoft.Azure.Cosmos;

namespace AzFncWebCounter.Methods;

public static partial class WebCounterMethod
{
    // If the database and the container does not exist, create them
    public static async Task CreateContainerIfNotExistsAsync(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
        await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");
    }

    // Check if this is the first time the function is run
    public static async Task<bool> IsFirstTimeAsync(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        bool trigger;
        // Get the container
        var container = cosmosClient.GetContainer(databaseName, containerName);

        // Get the count of items in the container
        var count = await container.GetItemQueryIterator<int>("SELECT VALUE COUNT(1) FROM c").ReadNextAsync();

        // If the count is 0, then this is the first time the function is run
        if (count.First() == 0)
        {
            trigger = true;
            // Create new record using the WebCounterModel
            var webCounterModel = new WebCounterModel
            {
                Counter = 1,
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };
            
            await container.CreateItemAsync(webCounterModel, new PartitionKey(webCounterModel.Id));
        }
        else
        {
            trigger = false;
        }

        return trigger;
    }

    // Get the current id, counter, and timestamp
    public static async Task<WebCounterModel> GetCountAsync(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        // Get the container
        var container = cosmosClient.GetContainer(databaseName, containerName);

        // Get the counter, id, and timestamp from the container
        var count = await container.GetItemQueryIterator<WebCounterModel>("SELECT c.id, c.counter FROM c").ReadNextAsync();

        // Assign count to a variable then increment it
        var currentCount = count.First();

        // Update the count in the database
        var webCounterModel = new WebCounterModel
        {
            Counter = currentCount.Counter + 1,
            Id = currentCount.Id,
            Timestamp = DateTime.UtcNow
            
        };
        await container.UpsertItemAsync(webCounterModel, new PartitionKey(webCounterModel.Id));

        // Return the count
        return currentCount; // currentCount.Counter; //count.First();
    }
}