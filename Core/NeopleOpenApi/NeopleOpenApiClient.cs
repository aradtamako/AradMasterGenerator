using Core.NeopleOpenApi.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Core.NeopleOpenApi
{
    public class NeopleOpenApiClient
    {
        private const string HttpClientName = "NeopleOpenApiClient";
        private const string NeopleOpenApiBaseAddress = "https://api.neople.co.kr";

        private static HttpClient Client { get; set; } = default!;
        private string[] ApiKeys { get; set; }
        private string ApiKey
        {
            get
            {
                int index = new Random((int)DateTime.Now.Ticks).Next(0, ApiKeys.Length);
                return ApiKeys[index];
            }
        }

        public NeopleOpenApiClient(IEnumerable<string> apiKeys)
        {
            ApiKeys = apiKeys.ToArray();

            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(x => x.StatusCode == HttpStatusCode.BadRequest)
                .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));

            var services = new ServiceCollection();
            services
                .AddHttpClient(HttpClientName, x =>
                {
                    x.BaseAddress = new Uri(NeopleOpenApiBaseAddress);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    UseProxy = Config.Config.Instance.Proxy.Enabled,
                    Proxy = new WebProxy(Config.Config.Instance.Proxy.Host, Config.Config.Instance.Proxy.Port)
                })
                .AddPolicyHandler(policy);

            var provider = services.AddHttpClient().BuildServiceProvider();
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            Client = factory.CreateClient(HttpClientName);
        }

        private async Task<T> Get<T>(string requestUri)
        {
            var response = await Client.GetAsync(requestUri).ConfigureAwait(false);
            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            var serializer = new JsonSerializer();
            using var streamReader = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(streamReader);
            return serializer.Deserialize<T>(jsonTextReader) ?? throw new InvalidDataException();
        }

        public async Task<Job[]> GetJobs()
            => (await Get<JobResponse>($"/df/jobs?apikey={ApiKey}").ConfigureAwait(false)).Jobs;

        public async Task<Skill[]> GetSkills(string jobId, string jobGrowId)
            => (await Get<SkillResponse>($"/df/skills/{jobId}?jobGrowId={jobGrowId}&apikey={ApiKey}").ConfigureAwait(false)).Skills;

        public async Task<SkillDetail> GetSkillDetail(string jobId, string skillId)
            => await Get<SkillDetail>($"/df/skills/{jobId}/{skillId}?&apikey={ApiKey}").ConfigureAwait(false);
    }
}
