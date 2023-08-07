using System;
using Newtonsoft.Json;

namespace AzFncWebCounter.Models
{
    public class WebCounterModel
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("counter")]
        public int Counter { get; set; } = 0;

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
