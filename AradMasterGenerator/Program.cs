using Core;
using Core.Config;
using Core.DnfOfficialWebSite;
using Core.Master.Model;
using Core.NeopleOpenApi;
using Core.NeopleOpenApi.Model;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AradMasterGenerator
{
    class Program
    {
        const string StringTableDirectoryName = "Resources/string_table";
        const string MasterDirectoryName = "master";
        const string SkillImageDirectoryName = "image/skill";
        const string JobImageDirectoryName = "image/job";

        static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        static async Task GenerateJobMaster(NeopleOpenApiClient neopleOpenApiClient)
        {
            var jobs = new List<Core.Master.Model.Job>();
            foreach (var job in await neopleOpenApiClient.GetJobs().ConfigureAwait(false))
            {
                jobs.Add(new Core.Master.Model.Job
                {
                    Id = job.JobId,
                    NameKor = job.JobName
                });

                foreach (var jobGrow in job.JobGrows ?? default!)
                {
                    var baseGrowId = jobGrow.JobGrowId;
                    var growCount = 1;
                    for (var nextJobGrow = jobGrow; nextJobGrow != null; nextJobGrow = nextJobGrow.Next)
                    {
                        jobs.Add(new Core.Master.Model.Job
                        {
                            Id = job.JobId,
                            BaseGrowId = baseGrowId,
                            GrowId = nextJobGrow.JobGrowId,
                            GrowCount = growCount++,
                            NameKor = job.JobName,
                            GrowNameKor = nextJobGrow.JobName
                        });
                    }
                }
            }

            File.WriteAllText($"{MasterDirectoryName}/jobs.json", JsonConvert.SerializeObject(jobs, Formatting.Indented));

            var dbJobIds = DB.Instance.Query<Core.Master.Model.Job>("select * from jobs").Select(x => $"{x.Id}{x.GrowId}");
            var diffJobs = jobs.Where(x => !dbJobIds.Contains($"{x.Id}{x.GrowId}"));
            if (diffJobs.Any())
            {
                DB.Instance.Insert(diffJobs);
            }
        }

        static async Task DownloadSkillIcon(HttpClient client, string url, string filePath)
        {
            if (!File.Exists(filePath))
            {
                CreateDirectoryIfNotExists(Path.GetDirectoryName(filePath) ?? default!);

                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var fs = File.OpenWrite(filePath);
                stream.CopyTo(fs);
            }
        }

        static async Task GenerateSkillMaster(NeopleOpenApiClient neopleOpenApiClient, DnfOfficialWebSiteClient dnfOfficialWebSiteClient)
        {
            var skills = new List<Core.Master.Model.Skill>();
            var skillIcons = await dnfOfficialWebSiteClient.GetSkillIcons();

            // 全職業共通スキル
            var commonSkills = new List<Core.Master.Model.Skill>
            {
                // バックステップ
                new Core.Master.Model.Skill { Id = "7822d6d52e10964a6755f142c666b494", NameKor = "백스텝", RequiredLevel = 1, Type = "active", CostType = "SP" },
                // クイックスタンディング
                new Core.Master.Model.Skill { Id = "ce26c6b69d02a440a81b552bec94f03b", NameKor = "퀵 스탠딩", RequiredLevel = 1, Type = "active", CostType = "SP" },
                // 基本技熟練
                new Core.Master.Model.Skill { Id = "5a56514f35cf0270ae8d6c65f8fefd78", NameKor = "기본기 숙련", RequiredLevel = 1, Type = "passive", CostType = "SP" },
                // 跳躍
                new Core.Master.Model.Skill { Id = "1fea5a626f15230237946a11a9d11582", NameKor = "도약", RequiredLevel = 10, Type = "active", CostType = "SP" },
                // クリティカルヒット
                new Core.Master.Model.Skill { Id = "fc1262c19f3d0477ee8eda47b8db8696", NameKor = "크리티컬 히트", RequiredLevel = 20, Type = "passive", CostType = "SP" },
                // バックアタック
                new Core.Master.Model.Skill { Id = "8a3b4a6cea49837706c36da3d9904f95", NameKor = "백 어택", RequiredLevel = 20, Type = "passive", CostType = "SP" },
                // 古代の記憶
                new Core.Master.Model.Skill { Id = "de13113fc6cb4c8880e8d985edb34aea", NameKor = "고대의 기억", RequiredLevel = 15, Type = "active", CostType = "SP" },
                // 不屈の意志
                new Core.Master.Model.Skill { Id = "dummy003", NameKor = "불굴의 의지", RequiredLevel = 15, Type = "active", CostType = "SP" },
                // 投擲マスタリー
                new Core.Master.Model.Skill { Id = "dummy004", NameKor = "투척 마스터리", RequiredLevel = 10, Type = "passive", CostType = "SP" },
                // コンバージョン
                new Core.Master.Model.Skill { Id = "12dca7fbf791e882b025a0d916181673", NameKor = "컨버전", RequiredLevel = 20, Type = "passive", CostType = "SP" }
            };

            foreach (var commonSkill in commonSkills)
            {
                var skillIcon = skillIcons.Where(x => x.Skill.NameKor.Replace("물리 ", "") == commonSkill.NameKor).FirstOrDefault();
                if (skillIcon == default)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(commonSkill, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii }));
                    continue;
                    // throw new InvalidDataException();
                }

                commonSkill.IconUrl = skillIcon.Skill.IconUrl;
                skills.Add(commonSkill);
            }

            foreach (var job in await neopleOpenApiClient.GetJobs().ConfigureAwait(false))
            {
                foreach (var jobGrow in job.JobGrows ?? default!)
                {
                    var baseGrowId = jobGrow.JobGrowId;
                    for (var nextJobGrow = jobGrow; nextJobGrow != null; nextJobGrow = nextJobGrow.Next)
                    {
                        foreach (var skill in await neopleOpenApiClient.GetSkills(job.JobId, nextJobGrow.JobGrowId).ConfigureAwait(false))
                        {
                            var skillIcon = skillIcons
                                .Where(x => x.Skill.NameKor.Replace(" ", "") == skill.Name.Replace(" ", ""))
                                .Where(x => x.JobId == job.JobId)
                                .Where(x => x.BaseGrowId == baseGrowId)
                                .FirstOrDefault();

                            if (skillIcon == default)
                            {
                                // 一部スキル（「刹那の悟り」など）はWebページから削除されているので職業IDを無視して検索する
                                skillIcon = skillIcons
                                    .Where(x => x.Skill.NameKor.Replace(" ", "") == skill.Name.Replace(" ", ""))
                                    .FirstOrDefault();

                                if (skillIcon == default)
                                {
                                    // アイコンが見つからなかった
                                    Console.WriteLine(JsonConvert.SerializeObject(skill, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii }));
                                    // throw new InvalidDataException();
                                }
                            }

                            // 一部スキルアイコンは公式サイト側が間違っているので修正する
                            if (skillIcon.Skill?.IconUrl == "http://d-fighter.dn.nexoncdn.co.kr/samsungdnf/neople/swf/2019/skill/7/icon/188.png")
                            {
                                skillIcon.Skill.IconUrl = "https://i.imgur.com/QndDlzz.png";
                            }

                            skills.Add(new Core.Master.Model.Skill
                            {
                                Id = skill.SkillId,
                                JobId = job.JobId,
                                JobGrowId = nextJobGrow.JobGrowId,
                                RequiredLevel = skill.RequiredLevel,
                                Type = skill.Type,
                                CostType = skill.CostType,
                                NameKor = skill.Name,
                                IconUrl = skillIcon.Skill?.IconUrl ?? null
                            });
                        }
                    }
                }
            }

            // i18n対応
            var skillStringTableDirectoryName = $"{StringTableDirectoryName}/skill";
            foreach (var fileName in new string[] { "kor_jpn.csv", "kor_eng.csv" })
            {
                Util.LoadStringTable($"{skillStringTableDirectoryName}/{fileName}");
                foreach (var skill in skills)
                {
                    var str = Util.GetString(skill.NameKor)?.Trim();
                    if (!string.IsNullOrEmpty(str))
                    {
                        if (fileName.Contains("jpn"))
                        {
                            skill.NameJpn = str;
                        }
                        else if (fileName.Contains("eng"))
                        {
                            skill.NameEng = str;
                        }
                    }
                }
            }

            // スキルアイコンをダウンロードする
            var client = new HttpClient();
            Parallel.ForEach(skills, new ParallelOptions { MaxDegreeOfParallelism = 10 }, skill =>
            {
                var filePath = new StringBuilder();
                filePath.Append($"{SkillImageDirectoryName}/");
                if (!string.IsNullOrEmpty(skill.JobId))
                {
                    filePath.Append($"{skill.JobId}/");
                }
                if (!string.IsNullOrEmpty(skill.JobGrowId))
                {
                    filePath.Append($"{skill.JobGrowId}/");
                }
                filePath.Append($"{skill.Id}{Path.GetExtension(skill.IconUrl ?? ".png")}");

                skill.IconPath = $"/{filePath}";

                if (string.IsNullOrWhiteSpace(skill.IconUrl))
                {
                    return;
                }

                DownloadSkillIcon(client, skill.IconUrl ?? default!, filePath.ToString()).ConfigureAwait(false).GetAwaiter().GetResult();
            });

            File.WriteAllText($"{MasterDirectoryName}/skills.json", JsonConvert.SerializeObject(skills, Formatting.Indented));
            DB.Instance.ExecuteSQL("truncate table skills");
            DB.Instance.Insert(skills);
        }

        static async Task DownloadJobIcon()
        {
            var jobs = DB.Instance.Query<Core.Master.Model.Job>("select * from jobs").ToArray();

            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision).ConfigureAwait(false);
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false
            }).ConfigureAwait(false);
            var page = await browser.NewPageAsync().ConfigureAwait(false);
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1920,
                Height = 1080
            }).ConfigureAwait(false);
            await page.GoToAsync("http://df.nexon.com/df/pg/character/srch").ConfigureAwait(false);
            var elements = await page.XPathAsync("//div[contains(@class, \"mix\")]").ConfigureAwait(false);

            foreach (var element in elements)
            {
                await element.EvaluateFunctionAsync("x => x.scrollIntoView()").ConfigureAwait(false);

                var jobNameKorElement = (await element.XPathAsync("a/div/p/strong").ConfigureAwait(false)).FirstOrDefault();
                var jobNameKor = (jobNameKorElement != null)
                    ? await jobNameKorElement.EvaluateFunctionAsync<string>("x => x.innerText").ConfigureAwait(false)
                    : string.Empty;
                var hasSex = (jobNameKor.Contains("(남)") || jobNameKor.Contains("(여)"));
                var sex = jobNameKor.Contains("(남)") ? "m" : "f";
                jobNameKor = jobNameKor.Replace("(남)", "").Replace("(여)", "").Replace(" ", "");

                var titleElement = (await element.XPathAsync("a/div/p").ConfigureAwait(false)).FirstOrDefault();
                if (titleElement != null)
                {
                    var jobQuery = jobs.Where(x => x.GrowNameKor?.Replace(" ", "") == jobNameKor || x.NameKor.Replace(" ", "") == jobNameKor);
                    if (hasSex)
                    {
                        jobQuery = jobQuery.Where(x => x.sex == sex);
                    }
                    var job = jobQuery.FirstOrDefault();

                    if (job == null)
                    {
                        continue;
                    }

                    await titleElement.EvaluateFunctionAsync("x => x.remove()").ConfigureAwait(false);
                    var path = $"{JobImageDirectoryName}/{job.Id}/{job.BaseGrowId ?? "0"}.png";
                    CreateDirectoryIfNotExists(Path.GetDirectoryName(path) ?? default!);
                    await element.ScreenshotAsync(path, new ScreenshotOptions { BurstMode = true }).ConfigureAwait(false);
                }
            }

            await browser.CloseAsync().ConfigureAwait(false);
        }

        static async Task GenerateCardMaster(NeopleOpenApiClient neopleOpenApiClient)
        {
            var filePath = $"{MasterDirectoryName}/cards1.json";
            var reg = new Regex("df/items/(.*$)");
            var cards = new List<CardDetail>();
            var client = new HttpClient();
            var html = new HtmlDocument();
            CardDetail? cardDetail = null;

            // ファイルが存在する場合は読み込む
            if (File.Exists(filePath))
            {
                cards = JsonConvert.DeserializeObject<CardDetail[]>(File.ReadAllText(filePath)).ToList();
            }

            html.LoadHtml(await client.GetStringAsync("http://dnfnow.xyz/magic?card_search=%EC%B9%B4%EB%93%9C").ConfigureAwait(false));
            var nodes = html.DocumentNode.SelectNodes("//table[@id=\"showtables\"]/tr/td/button");
            foreach (var node in nodes.Select((val, i) => new { val, i }))
            {
                Console.WriteLine($"{node.i + 1}/{nodes.Count}");

                var ignoreRarities = new string[] { "커먼", "언커먼" };
                var rarity = node.val.SelectSingleNode("span").InnerText;
                if (ignoreRarities.Contains(rarity))
                {
                    continue;
                }
                var imgSrc = node.val.SelectSingleNode("img").Attributes["src"].Value;
                var itemId = reg.Match(imgSrc).Groups[1].Value;

                var itemName = node.val.InnerText.Trim();
                itemName = itemName.Replace("&amp;", "&");
                itemName = itemName.Substring(0, itemName.IndexOf("카드") + 2);

                cardDetail = await neopleOpenApiClient.GetCardDetail(itemId).ConfigureAwait(false);
                if (!cards.Where(x => x.ItemId == cardDetail.ItemId).Any())
                {
                    cards.Add(cardDetail);
                }
            }

            // 任意のカードを追加する
            /*
            var ids = new string[]
            {
                "8c48f046bdf2bf059befeef5e75c1856",
                "0de18501dcb2cd1a60462b393a12947d",
            };

            foreach (var id in ids)
            {
                cardDetail = await neopleOpenApiClient.GetCardDetail(id).ConfigureAwait(false);
                if (!cards.Where(x => x.ItemId == cardDetail.ItemId).Any())
                {
                    cards.Add(cardDetail);
                }
            }
            */

            File.WriteAllText(filePath, JsonConvert.SerializeObject(cards, Formatting.Indented));
        }

        /// <summary>
        /// Convert to modern style
        /// </summary>
        static void ConvertCardMaster()
        {
            var cards = JArray.Parse(File.ReadAllText($"{MasterDirectoryName}/cards1.json"));
            foreach (JObject card in cards)
            {
                card["id"] = card["itemId"]!.Value<string>();
                card.Property("itemId")!.Remove();

                card["name_kor"] = card["itemName"]!.Value<string>();
                card["name_eng"] = "-";
                card["name_jpn"] = "-";
                card["name_zho"] = "-";
                card.Property("itemName")!.Remove();

                var rarities = new Dictionary<string, int>
                {
                    ["레어"] = 0, // Rare
                    ["유니크"] = 1, // Unique
                    ["레전더리"] = 2 // Legendary
                };
                card["rarity"] = rarities[card["itemRarity"]!.Value<string>()];
                card.Property("itemRarity")!.Remove();

                // cardInfo
                var cardSlots = new List<int>();
                foreach (JObject slot in card["cardInfo"]!["slots"]!)
                {
                    // https://i.imgur.com/lJWyK9w.png
                    var slots = new Dictionary<string, int>
                    {
                        ["SHOULDER"] = 0,
                        ["JACKET"] = 1,
                        ["PANTS"] = 2,
                        ["WAIST"] = 3,
                        ["SHOES"] = 4,
                        ["WEAPON"] = 5,
                        ["TITLE"] = 6,
                        ["WRIST"] = 7,
                        ["AMULET"] = 8,
                        ["SUPPORT"] = 9,
                        ["RING"] = 10,
                        ["EARRING"] = 11,
                        ["MAGIC_STON"] = 12,
                    };
                    cardSlots.Add(slots[slot["slotId"]!.Value<string>()]);
                }

                card["card_info"] = new JObject();
                card["card_info"]!["slots"] = JArray.FromObject(cardSlots);
                
                // enchant
                var statuses = new Dictionary<string, int>
                {
                    ["물리 공격력"] = 0,
                    ["마법 공격력"] = 1,
                    ["독립 공격력"] = 2,
                    ["물리 크리티컬 히트"] = 3,
                    ["마법 크리티컬 히트"] = 4,
                    ["화속성강화"] = 5,
                    ["수속성강화"] = 6,
                    ["명속성강화"] = 7,
                    ["암속성강화"] = 8,
                    ["모든 속성 강화"] = 9,
                    ["힘"] = 10,
                    ["지능"] = 11,
                    ["체력"] = 12,
                    ["정신력"] = 13,
                    ["공격속도"] = 14,
                    ["캐스트속도"] = 15,
                    ["이동속도"] = 16,
                    ["HP 1분당 회복"] = 17,
                    ["HP MAX"] = 18,
                    ["MP 1분당 회복"] = 19,
                    ["MP MAX"] = 20,
                    ["공격속성"] = 21,
                    ["모든 상태변화 내성"] = 22,
                    ["적중률"] = 23,
                    ["화속성저항"] = 24,
                    ["모든 속성 저항"] = 25,
                    ["회피율"] = 26,
                    ["히트리커버리"] = 27,
                    ["점프력"] = 28,
                };
                foreach (JObject enchant in card["cardInfo"]!["enchant"]!)
                {
                    foreach (JObject status in enchant["status"]!)
                    {
                        status["id"] = statuses[status["name"]!.Value<string>()];
                        status.Property("name")!.Remove();

                        var value = status["value"]!.Value<string>() switch
                        {
                            "명" => "LIGHT",
                            "수" => "WATER",
                            "암" => "SHADOW",
                            "화" => "FIRE",
                            _ => status["value"]!.Value<string>()
                        };
                        
                        status["value"] = value;
                    }
                }
                card["card_info"]!["enchant"] = card["cardInfo"]!["enchant"];

                card.Property("cardInfo")!.Remove();
            }

            // i18n対応
            var skillStringTableDirectoryName = $"{StringTableDirectoryName}/card";
            foreach (var fileName in new string[] { "jpn.csv" })
            {
                Util.LoadStringTable($"{skillStringTableDirectoryName}/{fileName}");
                foreach (var skill in cards)
                {
                    var str = Util.GetString(skill["id"]!.Value<string>())?.Trim();
                    if (!string.IsNullOrEmpty(str))
                    {
                        if (fileName.Contains("jpn"))
                        {
                            skill["name_jpn"] = str;
                        }
                        else if (fileName.Contains("eng"))
                        {
                            skill["name_eng"] = str;
                        }
                    }
                }
            }

            File.WriteAllText($"{MasterDirectoryName}/cards2.json", JsonConvert.SerializeObject(cards));
            File.WriteAllText($"{MasterDirectoryName}/cards3.csv", string.Join("\n", cards.Select(x => $"{x["id"]!.Value<string>()}\t{x["name_kor"]!.Value<string>()}")));
        }

        static async Task Main()
        {
            Console.OutputEncoding = Encoding.Unicode;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            CreateDirectoryIfNotExists(MasterDirectoryName);

            var neopleOpenApiClient = new NeopleOpenApiClient(Config.Instance.NeopleOpenApi.ApiKeys);
            var dnfOfficialWebSiteClient = new DnfOfficialWebSiteClient();

            //await GenerateJobMaster(neopleOpenApiClient).ConfigureAwait(false);
            //await DownloadJobIcon();
            //await GenerateSkillMaster(neopleOpenApiClient, dnfOfficialWebSiteClient).ConfigureAwait(false);
            await GenerateCardMaster(neopleOpenApiClient);
            ConvertCardMaster();
        }
    }
}
