using Core.NeopleOpenApi.Model;
using HtmlAgilityPack;
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
using System.Xml.Schema;

namespace Core.DnfOfficialWebSite
{
    public class DnfOfficialWebSiteClient
    {
        private const string HttpClientName = "DnfOfficialWebSiteClient";
        private const string DnfOffisialWebSiteBaseAddress = "http://df.nexon.com";

        private static HttpClient Client { get; set; } = default!;

        public DnfOfficialWebSiteClient()
        {
            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));

            var services = new ServiceCollection();
            services
                .AddHttpClient(HttpClientName, x =>
                {
                    x.BaseAddress = new Uri(DnfOffisialWebSiteBaseAddress);
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

        /// <summary>
        /// 各職業のURLを取得する
        /// </summary>
        private async Task<IEnumerable<string>> GetJobUrls()
        {
            var jobBasePath = "/df/guide/TO/1069";
            var str = await Client.GetStringAsync(jobBasePath).ConfigureAwait(false);
            var html = new HtmlDocument();
            html.LoadHtml(str);

            var urls = html.DocumentNode.SelectNodes("//ul[@class=\"comm_tab_sub\"]/li")
                .Select(x => x.SelectSingleNode("a")?.Attributes["href"].Value ?? "")
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            urls.Add($"{DnfOffisialWebSiteBaseAddress}{jobBasePath}");

            return urls;
        }

        /// <summary>
        /// スキルアイコンを取得する
        /// </summary>
        public async Task<List<Master.Model.Skill>> GetSkillIcons()
        {
            var skills = new List<Master.Model.Skill>();
            foreach (var url in await GetJobUrls().ConfigureAwait(false))
            {
                var str = await Client.GetStringAsync(url).ConfigureAwait(false);
                var html = new HtmlDocument();
                html.LoadHtml(str);

                foreach (var node in html.DocumentNode.SelectNodes("//table[@class=\"nttable\"]/tbody/tr"))
                {
                    if (node.ChildNodes.Where(x => x.Name.Contains("th")).Any())
                    {
                        // ヘッダを飛ばす
                        continue;
                    }

                    var skillIconUrl = node.SelectSingleNode("td[1]/img").Attributes["src"].Value;
                    var skillName = node.SelectSingleNode("td[2]")?.InnerText?.Replace("&amp;", "&") ?? "";

                    skills.Add(new Master.Model.Skill
                    {
                        NameKor = skillName,
                        IconUrl = skillIconUrl
                    });
                }
            }

            return skills;
        }
    }
}
