using Bardiche.JSONModels;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bardiche.Classes
{
    public static class RSSTorrentUtility
    {
        public static bool qBitTorrentAddFilter(string[] filters)
        {
            qBittorrentFilters items = null;
            qBittorrentSources sources = null;

            using (StreamReader r = new StreamReader(Extensions.config_values.qBittorrent_rss_filters))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<qBittorrentFilters>(json);
            }

            using (StreamReader r = new StreamReader(Extensions.config_values.qBittorrent_rss_sources))
            {
                string json = r.ReadToEnd();
                sources = JsonConvert.DeserializeObject<qBittorrentSources>(json);
            }

            if (items != null && sources != null)
            {
                foreach (string unparsed in filters)
                {
                    string input = unparsed.TrimStart(' ').ToLower();
                    if (!items.ContainsKey(input))
                    {
                        items.Add(input, new FilterConfig()
                        {
                            affectedFeeds = sources.Count == 0 ? new List<string>() : sources.Select(s => s.Value.url).ToList(),
                            mustContain = input,
                            savePath = Extensions.config_values.qBittorrent_save_path + input
                        });
                    }
                }
                File.WriteAllText(Extensions.config_values.qBittorrent_rss_filters, JsonConvert.SerializeObject(items));
                return true;
            }

            return false;
        }

        public static bool qBitTorrentRemoveFilter(string[] filters)
        {
            qBittorrentFilters items = null;

            using (StreamReader r = new StreamReader(Extensions.config_values.qBittorrent_rss_filters))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<qBittorrentFilters>(json);
            }

            if (items != null)
            {
                foreach (string unparsed in filters)
                {
                    string input = unparsed.TrimStart(' ').ToLower();
                    IEnumerable<string> matchingKeys = items.Keys.Where(k => k.Contains(input));
                    foreach (string removeKey in matchingKeys)
                        items.Remove(removeKey);
                }
                File.WriteAllText(Extensions.config_values.qBittorrent_rss_filters, JsonConvert.SerializeObject(items));
                return true;
            }

            return false;
        }

        public static bool qBitTorrentAddSource(string source)
        {
            qBittorrentFilters items = null;
            qBittorrentSources sources = null;

            using (StreamReader r = new StreamReader(Extensions.config_values.qBittorrent_rss_filters))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<qBittorrentFilters>(json);
            }

            using (StreamReader r = new StreamReader(Extensions.config_values.qBittorrent_rss_sources))
            {
                string json = r.ReadToEnd();
                sources = JsonConvert.DeserializeObject<qBittorrentSources>(json);
            }

            if (items != null && sources != null)
            {
                if (sources.Values.Where(v => v.url == source).Count() == 0)
                {
                    sources.Add(source, new SourceConfig() { url = source });
                    File.WriteAllText(Extensions.config_values.qBittorrent_rss_sources, JsonConvert.SerializeObject(sources));
                    foreach (KeyValuePair<string, FilterConfig> filter in items)
                    {
                        if (!filter.Value.affectedFeeds.Contains(source))
                            filter.Value.affectedFeeds.Add(source);
                    }
                    File.WriteAllText(Extensions.config_values.qBittorrent_rss_filters, JsonConvert.SerializeObject(items));
                }
                return true;
            }

            return false;
        }

        public static bool qBitTorrentRemoveSource(string source)
        {
            qBittorrentFilters items = null;
            qBittorrentSources sources = null;

            using (StreamReader r = new StreamReader(Extensions.config_values.qBittorrent_rss_filters))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<qBittorrentFilters>(json);
            }

            using (StreamReader r = new StreamReader(Extensions.config_values.qBittorrent_rss_sources))
            {
                string json = r.ReadToEnd();
                sources = JsonConvert.DeserializeObject<qBittorrentSources>(json);
            }

            if (items != null && sources != null)
            {
                if (sources.Values.Where(v => v.url == source).Count() > 0)
                {
                    foreach (KeyValuePair<string, FilterConfig> filter in items)
                        filter.Value.affectedFeeds.Remove(source);
                    File.WriteAllText(Extensions.config_values.qBittorrent_rss_filters, JsonConvert.SerializeObject(items));

                    foreach (KeyValuePair<string, SourceConfig> sourceItem in sources)
                    {
                        if (sourceItem.Value.url == source)
                            sources.Remove(sourceItem.Key);
                    }
                    File.WriteAllText(Extensions.config_values.qBittorrent_rss_sources, JsonConvert.SerializeObject(sources));
                }
                return true;
            }

            return false;
        }

        public static void qBittorrentRestart()
        {
            Process[] processes = Process.GetProcessesByName("qbittorrent");
            if (processes.Length > 0)
            {
                Process process = processes[0];
                string path = process.MainModule.FileName;
                process.Kill();
                Thread.Sleep(5000);
                Process.Start(path);
            }
        }
    }
}