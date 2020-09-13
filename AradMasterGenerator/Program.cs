using Core.Config;
using Core.NeopleOpenApi;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AradSkillMasterGenerator
{
    class Program
    {
        static async Task Main()
        {
            var client = new NeopleOpenApiClient(Config.Instance.NeopleOpenApi.ApiKey);
            var jobs = await client.GetJobList();
            Console.WriteLine(JsonConvert.SerializeObject(jobs));
        }
    }
}
