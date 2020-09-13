using Core.NeopleOpenApi.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Core.NeopleOpenApi
{
    public class NeopleOpenApiClient
    {
        private static HttpClient Client { get; set; } = default!;
        private string ApiKey { get; set; }

        public NeopleOpenApiClient(string apiKey)
        {
            ApiKey = apiKey;

            var services = new ServiceCollection();
            services
                .AddHttpClient("NeopleOpenApiClient", x =>
                {
                    x.BaseAddress = new Uri("https://api.neople.co.kr");
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    UseProxy = Config.Config.Instance.Proxy.Enabled,
                    Proxy = new WebProxy(Config.Config.Instance.Proxy.Host, Config.Config.Instance.Proxy.Port)
                })
                .AddTransientHttpErrorPolicy(x => x.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1)));

            var provider = services.AddHttpClient().BuildServiceProvider();
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            Client = factory.CreateClient("NeopleOpenApiClient");
        }

        public async Task<JobResponse> GetJobList()
        {
            var response = await Client.GetAsync($"/df/jobs?apikey={ApiKey}").ConfigureAwait(false);
            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            var serializer = new JsonSerializer();
            using var streamReader = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(streamReader);
            return serializer.Deserialize<JobResponse>(jsonTextReader) ?? throw new InvalidDataException();
        }
    }
}
