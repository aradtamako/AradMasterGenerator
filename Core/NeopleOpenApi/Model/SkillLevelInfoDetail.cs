using Newtonsoft.Json;

namespace Core.NeopleOpenApi.Model
{
    public class SkillLevelInfoDetail
    {
        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("consumeMp")]
        public int? ConsumeMP { get; set; }

        [JsonProperty("coolTime")]
        public double? CoolTime { get; set; }

        [JsonProperty("castingTime")]
        public double? CastingTime { get; set; }

        [JsonProperty("optionValue")]
        public SkillOptionValue OptionValue { get; set; } = default!;
    }
}
