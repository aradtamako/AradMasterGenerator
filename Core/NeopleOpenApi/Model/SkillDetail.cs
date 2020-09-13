using Newtonsoft.Json;

namespace Core.NeopleOpenApi.Model
{
    public class SkillDetail
    {
        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        [JsonProperty("type")]
        public string Type { get; set; } = default!;

        [JsonProperty("costType")]
        public string CostType { get; set; } = default!;

        [JsonProperty("desc")]
        public string Desc { get; set; } = default!;

        [JsonProperty("descDetail")]
        public string DescDetail { get; set; } = default!;

        [JsonProperty("consumeItem")]
        public SkillConsumeItem ConsumeItem { get; set; } = default!;

        [JsonProperty("maxLevel")]
        public int MaxLevel { get; set; }

        [JsonProperty("requiredLevel")]
        public int RequiredLevel { get; set; }

        [JsonProperty("requiredLevelRange")]
        public int RequiredLevelRange { get; set; }

        [JsonProperty("preRequiredSkill")]
        public PreRequiredSkill[]? PreRequiredSkill { get; set; }

        [JsonProperty("jobId")]
        public string JobId { get; set; } = default!;

        [JsonProperty("jobName")]
        public string JobName { get; set; } = default!;

        [JsonProperty("jobGrowLevel")]
        public SkillJobGrowLevel[]? JobGrowLevel { get; set; }

        [JsonProperty("levelInfo")]
        public SkillLevelInfo LevelInfo { get; set; } = default!;
    }
}
