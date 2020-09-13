using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.NeopleOpenApi.Model
{
    public class Skill
    {
        [JsonProperty("skillId")]
        public string SkillId { get; set; } = default!;

        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        [JsonProperty("requiredLevel")]
        public int RequiredLevel { get; set; } = default!;

        [JsonProperty("type")]
        public string Type { get; set; } = default!;

        [JsonProperty("costType")]
        public string CostType { get; set; } = default!;
    }
}
