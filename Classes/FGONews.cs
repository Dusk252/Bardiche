using Bardiche.Properties;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bardiche.Classes
{
    public class FGONews
    {
        private List<string> known_guid = new List<string>();
        private Dictionary<string, string> sources;
        HtmlWeb web = new HtmlWeb();
        private static HttpClient client = new HttpClient();
        private string webhook = Extensions.config_values.webhook_url;
        private List<string> maint_triggers = new List<string>() { "■日時", "▼ゲームの更新", "【対応内容】" };
        private string urlBase = "https://news.fate-go.jp";

        public FGONews()
        {
            sources = new Dictionary<string, string>() { {"お知らせ", @"https://news.fate-go.jp" }, { "メンテナンス", @"https://news.fate-go.jp/maintenance" } };
            known_guid = File.ReadLines(Path.GetFullPath(Resources.fgo_news, Extensions.config_values.root_path)).Reverse().Take(60).ToList();
            web.OverrideEncoding = Encoding.UTF8;
        }

        public async void readNews()
        {
            while (true)
            {
                foreach (KeyValuePair<string, string> source in sources)
                {
                    List<string> temp = new List<string>();
                    try
                    {
                        var htmlDoc = web.Load(source.Value);
                        var newsNodes = htmlDoc.DocumentNode
                            .QuerySelectorAll("ul.list_news li");
                        foreach (HtmlNode node in newsNodes)
                        {
                            if (node.NodeType == HtmlNodeType.Element)
                            {
                                string date = node.QuerySelector("p.date").FirstChild.InnerText;
                                string title = node.QuerySelector("p.title").FirstChild.InnerText;
                                string hash = (date + title);
                                if (known_guid.Contains(hash)) break;
                                string url = "";
                                HtmlNode linkNode = node.QuerySelector("a");
                                if (linkNode != null)
                                {
                                    url = urlBase + node.QuerySelector("a").Attributes["href"].Value;
                                    await SendHook(node, source.Key, temp, date, title, url, hash);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error on html parsing." + e.Message);
                    }
                    temp.Reverse();
                    foreach (string s in temp)
                    {
                        File.AppendAllText(Path.GetFullPath(Resources.fgo_news, Extensions.config_values.root_path), Environment.NewLine + s);
                    }
                    if (known_guid.Count > temp.Count)
                        known_guid.RemoveRange(0, temp.Count);
                    known_guid.AddRange(temp);
                }
                await Task.Delay(55555);
            }
        }

        private async Task SendHook(HtmlNode node, string type, List<string> temp, string date, string title, string url, string hash)
        {
            string img_url = "";
            string description = "";
            int color = string.Compare(type, "お知らせ") == 0 ? 4886754 : 16098851;

            if (!String.IsNullOrWhiteSpace(url))
            {
                if (String.Compare(type, "お知らせ") == 0)
                {
                    var htmlDoc = web.Load(url);
                    var imgNode = htmlDoc.QuerySelector(".article img");
                    if (imgNode != null)
                        img_url = urlBase + imgNode.Attributes["src"].Value;
                }
                else if (String.Compare(type, "メンテナンス") == 0)
                {
                    var htmlDoc = web.Load(url);
                    var pNodes = htmlDoc.QuerySelectorAll(".article p");
                    StringBuilder sb = new StringBuilder();
                    string text;
                    foreach (HtmlNode pNode in pNodes)
                    {
                        if (node.NodeType == HtmlNodeType.Element)
                        {
                            text = pNode.InnerText;
                            foreach (string trigger in maint_triggers)
                            {
                                if (text.Contains(trigger))
                                {
                                    string[] split_text = text.Split(new string[] { "\n\n" }, StringSplitOptions.None);
                                    foreach (string s in split_text)
                                    {
                                        if (s.Contains(trigger))
                                        {
                                            text = Regex.Replace(s, @"((?:★5\(SSR\)BB))", "");
                                            text = Regex.Replace(text, @"(※対象のサーヴァント(?:.|\n)*?)(\d|$)", "$2");
                                            sb.Append(text.TrimEnd()).AppendLine().AppendLine();
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    description = sb.ToString();
                }

                var content = new
                {
                    username = "Paper Moon",
                    avatar_url = Resources.chaldea_logo,
                    embeds = new[] {
                    new
                    {
                        title = "**" + title + "**",
                        description = description,
                        url = url,
                        color = color,
                        image = new
                        {
                            url = img_url
                        },
                        author = new
                        {
                            name = "【FGO】" + date
                        }
                    }
                    }
                };

                StringContent request = new StringContent(JsonConvert.SerializeObject(content).ToString(),
                Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response;
                    do
                    {
                        response = await client.PostAsync(webhook, request).ConfigureAwait(false);
                        Thread.Sleep(500);
                    } while (response.StatusCode.ToString() == "BadRequest");
                    temp.Add(hash);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error on webhook request." + e.Message);
                }

            }
        }
    }
}
