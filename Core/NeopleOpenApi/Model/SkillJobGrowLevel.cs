using Newtonsoft.Json;

namespace Core.NeopleOpenApi.Model
{
    public class SkillJobGrowLevel
    {
        [JsonProperty("jobGrowId")]
        public string JobGrowId { get; set; } = default!;

        [JsonProperty("jobGrowName")]
        public string JobGrowName { get; set; } = default!;

        [JsonProperty("masterLevel")]
        public int MasterLevel { get; set; }
    }
}
