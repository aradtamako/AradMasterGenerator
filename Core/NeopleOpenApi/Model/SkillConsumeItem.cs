using Newtonsoft.Json;

namespace Core.NeopleOpenApi.Model
{
    public class SkillConsumeItem
    {
        [JsonProperty("itemId")]
        public string ItemId { get; set; } = default!;

        [JsonProperty("itemName")]
        public string ItemName { get; set; } = default!;

        [JsonProperty("value")]
        public int Value { get; set; }
    }
}
