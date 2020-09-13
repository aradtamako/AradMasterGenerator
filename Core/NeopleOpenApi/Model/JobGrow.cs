using Newtonsoft.Json;

namespace Core.NeopleOpenApi.Model
{
    public class JobGrow
    {
        [JsonProperty("jobGrowId")]
        public string JobGrowId { get; set; } = default!;

        [JsonProperty("jobGrowName")]
        public string JobName { get; set; } = default!;

        [JsonProperty("next")]
        public JobGrow? Next { get; set; }
    }
}
