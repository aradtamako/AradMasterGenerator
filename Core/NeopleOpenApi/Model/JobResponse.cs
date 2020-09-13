using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.NeopleOpenApi.Model
{
    public class JobResponse
    {
        [JsonProperty("rows")]
        public Job[] Jobs { get; set; } = default!;
    }
}
