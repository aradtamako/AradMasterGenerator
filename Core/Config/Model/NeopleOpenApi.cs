using Newtonsoft.Json;

namespace Core.Config.Model
{
    public class NeopleOpenApi
    {
        [JsonProperty("api_keys")]
        public string[] ApiKeys { get; set; } = default!;
    }
}
