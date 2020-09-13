using Newtonsoft.Json;

namespace Core.Config.Model
{
    public class NeopleOpenApi
    {
        [JsonProperty("api_key")]
        public string ApiKey { get; set; } = default!;
    }
}
