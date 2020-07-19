using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bardiche.Properties;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Http;
using System.Globalization;
using Discord.WebSocket;
using Discord;

namespace Bardiche.Classes
{
    public class RSSfeed
    {
        private List<string> known_guid = new List<string>();
        private static HttpClient client = new HttpClient();
        private string webhook = Extensions.config_values.webhook_url;

        private static List<string> a_filters;
        private static List<string> m_filters;
        private static List<string> a_sources;
        private static List<string> m_sources;
        private static List<string> g_sources;

        public RSSfeed()
        {
            Console.WriteLine("Starting up RSS feed...");
            known_guid = File.ReadLines(Path.GetFullPath(Resources.nyaa_log, Extensions.config_values.root_path)).Reverse().Take(30).ToList();
            RSSRefresh();
        }

        public async void ReadRSS()
        {
            while (true)
            {
                foreach (string source in a_sources)
                {
                    await SendHook(source, true, a_filters).ConfigureAwait(false);
                }
                foreach (string source in m_sources)
                {
                    await SendHook(source, true, m_filters).ConfigureAwait(false);
                }
                foreach (string source in g_sources)
                {
                    await SendHook(source, false, new List<string>()).ConfigureAwait(false);
                }
                await Task.Delay(77777);
            }
        }

        public static void RSSRefresh()
        {
            a_filters = Extensions.readFile(Path.GetFullPath(Resources.anime_filters, Extensions.config_values.root_path));
            m_filters = Extensions.readFile(Path.GetFullPath(Resources.manga_filters, Extensions.config_values.root_path));
            a_sources = Extensions.readFile(Path.GetFullPath(Resources.anime_sources, Extensions.config_values.root_path));
            m_sources = Extensions.readFile(Path.GetFullPath(Resources.manga_sources, Extensions.config_values.root_path));
            g_sources = Extensions.readFile(Path.GetFullPath(Resources.general_sources, Extensions.config_values.root_path));
        }

        private async Task SendHook(string url, bool check_filters, List<string> filters)
        {
            try
            {
                string rssData;
                try
                {
                    rssData = await client.GetStringAsync(url).ConfigureAwait(false);
                }
                catch
                {
                    return;
                }
                var feedXML = XDocument.Parse(rssData);

                var feeds = from feed in feedXML.Descendants("item")
                            select new
                            {
                                title = feed.Element("title").Value,
                                link = feed.Element("link").Value
                            };

                List<string> temp = new List<string>();

                foreach (var item in feeds)
                {
                    if (known_guid.Contains(item.link))
                    {
                        break;
                    }

                    var content = new
                    {
                        content = item.title + "\n" + item.link
                    };

                    if (check_filters)
                    {
                        int flag = 0;
                        foreach (var filter in filters)
                        {
                            CultureInfo culture = new CultureInfo("en-US");
                            flag = 1;
                            string[] check = filter.Split(' ');
                            foreach (var s in check)
                            {
                                if (culture.CompareInfo.IndexOf(item.title, s, CompareOptions.IgnoreCase) < 0)
                                {
                                    flag = 0;
                                    break;
                                }
                            }
                            if (flag == 1)
                            {
                                break;
                            }
                        }

                        if (flag == 0)
                        {
                            continue;
                        }
                    }

                    StringContent request = new StringContent(JsonConvert.SerializeObject(content).ToString(),
                                Encoding.UTF8, "application/json");
                    //string itemJson = JsonConvert.SerializeObject(msg);

                    try
                    {
                        HttpResponseMessage response;
                        do
                        {
                            response = await client.PostAsync(webhook, request).ConfigureAwait(false);
                            Thread.Sleep(500);
                        } while (response.StatusCode.ToString() == "BadRequest");

                        temp.Add(item.link);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0}: {1}", "RSS webhook error", e.StackTrace);
                    }
                }
                temp.Reverse();
                foreach (string s in temp)
                {
                    File.AppendAllText(Path.GetFullPath(Resources.nyaa_log, Extensions.config_values.root_path), Environment.NewLine + s);
                }
                if (known_guid.Count > temp.Count)
                    known_guid.RemoveRange(0, temp.Count);
                known_guid.AddRange(temp);

                return;
            }
            catch
            {
                return;
            }
        }

    }

}
