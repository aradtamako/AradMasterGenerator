using Newtonsoft.Json;

namespace Core.NeopleOpenApi.Model
{
    public class SkillLevelInfo
    {
        [JsonProperty("optionDesc")]
        public string OptionDesc { get; set; } = default!;

        [JsonProperty("rows")]
        public SkillLevelInfoDetail[] Option { get; set; } = default!;
    }
}
