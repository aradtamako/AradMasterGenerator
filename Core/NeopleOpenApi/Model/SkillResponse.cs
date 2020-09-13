using Newtonsoft.Json;

namespace Core.NeopleOpenApi.Model
{
    public class SkillResponse
    {
        [JsonProperty("skills")]
        public Skill[] Skills { get; set; } = default!;
    }
}
