using Newtonsoft.Json;

namespace Core.NeopleOpenApi.Model
{
    public class JobResponse
    {
        [JsonProperty("rows")]
        public Job[] Jobs { get; set; } = default!;
    }
}
