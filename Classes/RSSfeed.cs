using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bardiche.Properties;
using System.IO;
using System.Net.Http;
using System.Globalization;
using Discord;

namespace Bardiche.Classes
{
    public class RSSfeed
    {
        private List<string> known_guid = new List<string>();
        private static HttpClient client = new HttpClient();
        //private string webhook = Extensions.config_values.webhook_url;

        private static List<string> a_filters;
        private static List<string> a_sources;
        private static List<string> g_sources;

        public RSSfeed()
        {
            Console.WriteLine("Starting up RSS feed...");
            known_guid = File.ReadLines(Resources.rss_log).Reverse().Take(50).ToList();
            RSSRefresh();
        }

        public async Task ReadRSS(ITextChannel channel)
        {
            while (true)
            {
                foreach (string source in g_sources)
                {
                    await SendItems(source, false, new List<string>(), channel).ConfigureAwait(false);
                }
                foreach (string source in a_sources)
                {
                    await SendItems(source, true, a_filters, channel).ConfigureAwait(false);
                }
                await Task.Delay(77777);
            }
        }

        public static void RSSRefresh()
        {
            a_filters = Extensions.readRSS(Resources.rss_filters);
            a_sources = Extensions.readRSS(Resources.filtered_sources);
            g_sources = Extensions.readRSS(Resources.general_sources);
        }

        private async Task SendItems(string url, bool check_filters, List<string> filters, ITextChannel channel)
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

                    string content = item.title + "\n" + item.link;

                    /*var content = new
                    {
                        content = item.title + "\n" + item.link
                    };*/

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

                    /*StringContent request = new StringContent(JsonConvert.SerializeObject(content).ToString(),
                                Encoding.UTF8, "application/json");*/
                    //string itemJson = JsonConvert.SerializeObject(msg);

                    try
                    {
                        await channel.SendMessageAsync(content);
                        /*HttpResponseMessage response;
                        do
                        {
                            response = await client.PostAsync(webhook, request).ConfigureAwait(false);
                            Thread.Sleep(500);
                        } while (response.StatusCode.ToString() == "Bad Request");*/
                        temp.Add(item.link);
                    }
                    catch
                    {
                        Console.WriteLine("An error occurred with the RSS service.\nPlease make sure your rss channel id field in config.json is set correctly.");
                    }
                }
                temp.Reverse();
                foreach (string s in temp)
                {
                    File.AppendAllText(Resources.rss_log, s + Environment.NewLine);
                }
                known_guid.AddRange(temp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

    }

}
