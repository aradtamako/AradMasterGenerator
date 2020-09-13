using Newtonsoft.Json;

namespace Core.NeopleOpenApi.Model
{
    public class PreRequiredSkill
    {
        [JsonProperty("skillId")]
        public string SkillId { get; set; } = default!;

        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        [JsonProperty("level")]
        public string Level { get; set; } = default!;
    }
}
