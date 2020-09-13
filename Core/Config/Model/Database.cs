using Newtonsoft.Json;

namespace Core.Config.Model
{
    public class Database
    {
        [JsonProperty("connection_string")]
        public string ConnectionString { get; set; } = default!;
    }
}
