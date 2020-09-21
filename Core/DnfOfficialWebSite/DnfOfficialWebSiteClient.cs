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
        private (string JobId, string BaseGrowId, string JobName, string Url)[] GetJobUrlInfos()
        {
            return new (string JobId, string BaseGrowId, string JobName, string Url)[]
            {
                (string.Empty, string.Empty, "全職業共通スキル", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1069"),
                ("41f1cdc2ff58bb5fdc287be0db2a8df3", "df3870efe8e8754011cd12fa03cd275f", "鬼剣士（男） ウェポンマスター", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1008"),
                ("41f1cdc2ff58bb5fdc287be0db2a8df3", "1ea78ae210f681a799feb4403a5c1e85", "鬼剣士（男） ソウルブリンガー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1009"),
                ("41f1cdc2ff58bb5fdc287be0db2a8df3", "a9a4ef4552d46e39cf6c874a51126410", "鬼剣士（男） バーサーカー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1010"),
                ("41f1cdc2ff58bb5fdc287be0db2a8df3", "4a1459a4fa3c7f59b6da2e43382ed0b9", "鬼剣士（男） 阿修羅", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1011"),
                ("41f1cdc2ff58bb5fdc287be0db2a8df3", "a59ba19824dc3292b6075e29b3862ad3", "鬼剣士（男） 剣鬼", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1012"),
                ("a7a059ebe9e6054c0644b40ef316d6e9", "df3870efe8e8754011cd12fa03cd275f", "格闘家（女） ネンマスター", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1013"),
                ("a7a059ebe9e6054c0644b40ef316d6e9", "1ea78ae210f681a799feb4403a5c1e85", "格闘家（女） ストライカー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1014"),
                ("a7a059ebe9e6054c0644b40ef316d6e9", "a9a4ef4552d46e39cf6c874a51126410", "格闘家（女） 喧嘩屋", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1015"),
                ("a7a059ebe9e6054c0644b40ef316d6e9", "4a1459a4fa3c7f59b6da2e43382ed0b9", "格闘家（女） グラップラー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1016"),
                ("afdf3b989339de478e85b614d274d1ef", "df3870efe8e8754011cd12fa03cd275f", "ガンナー（男） レンジャー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1017"),
                ("afdf3b989339de478e85b614d274d1ef", "1ea78ae210f681a799feb4403a5c1e85", "ガンナー（男） ランチャー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1018"),
                ("afdf3b989339de478e85b614d274d1ef", "a9a4ef4552d46e39cf6c874a51126410", "ガンナー（男） メカニック", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1019"),
                ("afdf3b989339de478e85b614d274d1ef", "4a1459a4fa3c7f59b6da2e43382ed0b9", "ガンナー（男） スピッドファイア", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1020"),
                ("3909d0b188e9c95311399f776e331da5", "df3870efe8e8754011cd12fa03cd275f", "メイジ（女） エレメンタルマスター", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1021"),
                ("3909d0b188e9c95311399f776e331da5", "1ea78ae210f681a799feb4403a5c1e85", "メイジ（女） サモナー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1022"),
                ("3909d0b188e9c95311399f776e331da5", "a9a4ef4552d46e39cf6c874a51126410", "メイジ（女） バトルメイジ", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1023"),
                ("3909d0b188e9c95311399f776e331da5", "4a1459a4fa3c7f59b6da2e43382ed0b9", "メイジ（女） 魔導学者", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1024"),
                ("3909d0b188e9c95311399f776e331da5", "a59ba19824dc3292b6075e29b3862ad3", "メイジ（女） エンチャントレス", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1025"),
                ("f6a4ad30555b99b499c07835f87ce522", "df3870efe8e8754011cd12fa03cd275f", "プリースト（男） クルセイダー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1026"),
                ("f6a4ad30555b99b499c07835f87ce522", "1ea78ae210f681a799feb4403a5c1e85", "プリースト（男） インファイター", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1027"),
                ("f6a4ad30555b99b499c07835f87ce522", "a9a4ef4552d46e39cf6c874a51126410", "プリースト（男） 退魔士", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1028"),
                ("f6a4ad30555b99b499c07835f87ce522", "4a1459a4fa3c7f59b6da2e43382ed0b9", "プリースト（男） アベンジャー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1029"),
                ("944b9aab492c15a8474f96947ceeb9e4", "df3870efe8e8754011cd12fa03cd275f", "ガンナー（女） レンジャー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1030"),
                ("944b9aab492c15a8474f96947ceeb9e4", "1ea78ae210f681a799feb4403a5c1e85", "ガンナー（女） ランチャー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1031"),
                ("944b9aab492c15a8474f96947ceeb9e4", "a9a4ef4552d46e39cf6c874a51126410", "ガンナー（女） メカニック", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1032"),
                ("944b9aab492c15a8474f96947ceeb9e4", "4a1459a4fa3c7f59b6da2e43382ed0b9", "ガンナー（女） スピッドファイア", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1033"),
                ("ddc49e9ad1ff72a00b53c6cff5b1e920", "df3870efe8e8754011cd12fa03cd275f", "シーフ ローグ", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1034"),
                ("ddc49e9ad1ff72a00b53c6cff5b1e920", "1ea78ae210f681a799feb4403a5c1e85", "シーフ 死霊術師", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1035"),
                ("ddc49e9ad1ff72a00b53c6cff5b1e920", "a9a4ef4552d46e39cf6c874a51126410", "シーフ くノ一", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1036"),
                ("ddc49e9ad1ff72a00b53c6cff5b1e920", "4a1459a4fa3c7f59b6da2e43382ed0b9", "シーフ シャドウダンサー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1037"),
                ("ca0f0e0e9e1d55b5f9955b03d9dd213c", "df3870efe8e8754011cd12fa03cd275f", "格闘家（男） ネンマスター", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1038"),
                ("ca0f0e0e9e1d55b5f9955b03d9dd213c", "1ea78ae210f681a799feb4403a5c1e85", "格闘家（男） ストライカー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1039"),
                ("ca0f0e0e9e1d55b5f9955b03d9dd213c", "a9a4ef4552d46e39cf6c874a51126410", "格闘家（男） 喧嘩屋", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1040"),
                ("ca0f0e0e9e1d55b5f9955b03d9dd213c", "4a1459a4fa3c7f59b6da2e43382ed0b9", "格闘家（男） グラップラー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1041"),
                ("a5ccbaf5538981c6ef99b236c0a60b73", "df3870efe8e8754011cd12fa03cd275f", "メイジ（男） エレメンタルボマー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1042"),
                ("a5ccbaf5538981c6ef99b236c0a60b73", "1ea78ae210f681a799feb4403a5c1e85", "メイジ（男） 氷結士", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1043"),
                ("a5ccbaf5538981c6ef99b236c0a60b73", "a9a4ef4552d46e39cf6c874a51126410", "メイジ（男） ブラッドメイジ", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1044"),
                ("a5ccbaf5538981c6ef99b236c0a60b73", "4a1459a4fa3c7f59b6da2e43382ed0b9", "メイジ（男） スイフトマスター", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1045"),
                ("a5ccbaf5538981c6ef99b236c0a60b73", "a59ba19824dc3292b6075e29b3862ad3", "メイジ（男） ディメンションウォーカー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1046"),
                ("17e417b31686389eebff6d754c3401ea", "4fdee159d5aa8874a1459861ced676ec", "ダークナイト 自覚1", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1047"),
                ("b522a95d819a5559b775deb9a490e49a", "4fdee159d5aa8874a1459861ced676ec", "クリエイター 自覚1", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1048"),
                ("1645c45aabb008c98406b3a16447040d", "df3870efe8e8754011cd12fa03cd275f", "鬼剣士（女） ソードマスター", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1049"),
                ("1645c45aabb008c98406b3a16447040d", "1ea78ae210f681a799feb4403a5c1e85", "鬼剣士（女） ダークテンプラー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1050"),
                ("1645c45aabb008c98406b3a16447040d", "a9a4ef4552d46e39cf6c874a51126410", "鬼剣士（女） デーモンスレイヤー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1051"),
                ("1645c45aabb008c98406b3a16447040d", "4a1459a4fa3c7f59b6da2e43382ed0b9", "鬼剣士（女） バガボンド", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1052"),
                ("0ee8fa5dc525c1a1f23fc6911e921e4a", "df3870efe8e8754011cd12fa03cd275f", "ナイト エルブンナイト", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1053"),
                ("0ee8fa5dc525c1a1f23fc6911e921e4a", "1ea78ae210f681a799feb4403a5c1e85", "ナイト カオス", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1054"),
                ("0ee8fa5dc525c1a1f23fc6911e921e4a", "a9a4ef4552d46e39cf6c874a51126410", "ナイト パラディン", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1055"),
                ("0ee8fa5dc525c1a1f23fc6911e921e4a", "4a1459a4fa3c7f59b6da2e43382ed0b9", "ナイト ドラゴンナイト", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1056"),
                ("3deb7be5f01953ac8b1ecaa1e25e0420", "df3870efe8e8754011cd12fa03cd275f", "魔槍士 バンガード", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1057"),
                ("3deb7be5f01953ac8b1ecaa1e25e0420", "1ea78ae210f681a799feb4403a5c1e85", "魔槍士 デュエリスト", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1058"),
                ("3deb7be5f01953ac8b1ecaa1e25e0420", "a9a4ef4552d46e39cf6c874a51126410", "魔槍士 ドラゴニアンランサー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1060"),
                ("3deb7be5f01953ac8b1ecaa1e25e0420", "4a1459a4fa3c7f59b6da2e43382ed0b9", "魔槍士 ダークランサー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1059"),
                ("0c1b401bb09241570d364420b3ba3fd7", "df3870efe8e8754011cd12fa03cd275f", "プリースト（女） クルセイダー", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1061"),
                ("0c1b401bb09241570d364420b3ba3fd7", "1ea78ae210f681a799feb4403a5c1e85", "プリースト（女） 異端審問官", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1062"),
                ("0c1b401bb09241570d364420b3ba3fd7", "a9a4ef4552d46e39cf6c874a51126410", "プリースト（女） 巫女", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1063"),
                ("0c1b401bb09241570d364420b3ba3fd7", "4a1459a4fa3c7f59b6da2e43382ed0b9", "プリースト（女） ミストレス", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1064"),
                ("986c2b3d72ee0e4a0b7fcfbe786d4e02", "df3870efe8e8754011cd12fa03cd275f", "ガンブレーダー ヒットマン", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1065"),
                ("986c2b3d72ee0e4a0b7fcfbe786d4e02", "1ea78ae210f681a799feb4403a5c1e85", "ガンブレーダー エージェント", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1066"),
                ("986c2b3d72ee0e4a0b7fcfbe786d4e02", "a9a4ef4552d46e39cf6c874a51126410", "ガンブレーダー トラブルシューター", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1067"),
                ("986c2b3d72ee0e4a0b7fcfbe786d4e02", "4a1459a4fa3c7f59b6da2e43382ed0b9", "ガンブレーダー スペシャリスト", $"{DnfOffisialWebSiteBaseAddress}/df/guide/TO/1068")
            };
        }

        /// <summary>
        /// スキルアイコンを取得する
        /// </summary>
        public async Task<List<(string JobId, string BaseGrowId, string JobName, Master.Model.Skill Skill)>> GetSkillIcons()
        {
            var skills = new List<(string JobId, string BaseGrowId, string JobName, Master.Model.Skill Skill)>();
            foreach (var urlInfo in GetJobUrlInfos())
            {
                var str = await Client.GetStringAsync(urlInfo.Url).ConfigureAwait(false);
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

                    if (skillIconUrl.StartsWith("//"))
                    {
                        skillIconUrl = $"http:{skillIconUrl}";
                    }

                    skills.Add(new (urlInfo.JobId, urlInfo.BaseGrowId, urlInfo.JobName, new Master.Model.Skill
                    {
                        NameKor = skillName,
                        IconUrl = skillIconUrl
                    }));
                }
            }

            return skills;
        }
    }
}
