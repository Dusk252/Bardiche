using Bardiche.Classes;
using Bardiche.Properties;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bardiche.Modules
{
    public class RSSControlModule : ModuleBase
    {

        [Command("rssadd")]
        [Summary("Adds a new filter to the rss.")]
        public async Task RSSAdd([Remainder] string query)
        {
            string[] items = query.Split(new[] { "|" }, 2, StringSplitOptions.RemoveEmptyEntries);
            string[] filters = items[0].TrimEnd(' ').Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            List<string> existing = new List<string>();

            string mode;
            string resource;
            if (filters[0].Substring(0, 2).Equals("a "))
            {
                resource = Path.GetFullPath(Resources.anime_filters, Extensions.config_values.root_path);
                mode = filters[0].Substring(0, 1);
                filters[0] = filters[0].Substring(2);
            }
            else if (filters[0].Substring(0, 2).Equals("m "))
            {
                resource = Path.GetFullPath(Resources.manga_filters, Extensions.config_values.root_path);
                mode = filters[0].Substring(0, 1);
                filters[0] = filters[0].Substring(2);
            }
            else
            {
                resource = Path.GetFullPath(Resources.anime_filters, Extensions.config_values.root_path);
                mode = "a";
            }

            List<string> current = Extensions.readFile(resource);
            foreach (string unparsed in filters)
            {
                string input = unparsed.TrimStart(' ').ToLower();
                if (current.Contains(input))
                {
                    existing.Add(input);
                }
                else
                {
                    current.Add(input);
                }
            }
            if (existing.Count < filters.Length)
            {
                if (existing.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var wrong in existing)
                    {
                        sb.Append(", " + wrong);
                    }
                    await ReplyAsync("``" + sb.ToString().Substring(2) + (existing.Count == 1 ? " is " : " are ") + "already registered.``").ConfigureAwait(false);
                }
                if (mode == "a")
                {
                    if (RSSTorrentUtility.qBitTorrentAddFilter(filters))
                    {
                        Extensions.writeLinesToFile(current, resource);
                        RSSfeed.RSSRefresh();
                        await ReplyAsync("``The RSS filter has been updated.``").ConfigureAwait(false);
                        RSSTorrentUtility.qBittorrentRestart();
                    }
                    else
                        await ReplyAsync("``There was an issue updating the qBitTorrent autodownload config.``").ConfigureAwait(false);
                }
                else
                {
                    Extensions.writeLinesToFile(current, resource);
                    RSSfeed.RSSRefresh();
                    await ReplyAsync("``The RSS filter has been updated.``").ConfigureAwait(false);
                }
            }
            else
            {
                await ReplyAsync("``All the filters you tried to add already exist.``").ConfigureAwait(false);
            }

            if (items.Length == 2 && items[1].TrimStart(' ').Equals("s"))
            {
                await RSSShow(mode);
            }
        }

        [Command("rssshow")]
        [Summary("Shows list of shows in the rss filter.")]
        public async Task RSSShow([Remainder] string query = "a")
        {
            List<string> current;
            if (query.Equals("a"))
            {
                current = Extensions.readFile(Path.GetFullPath(Resources.anime_filters, Extensions.config_values.root_path));
            }
            else if (query.Equals("m"))
            {
                current = Extensions.readFile(Path.GetFullPath(Resources.manga_filters, Extensions.config_values.root_path));
            }
            else
            {
                current = Extensions.readFile(Path.GetFullPath(Resources.anime_filters, Extensions.config_values.root_path));
            }
            StringBuilder sb = new StringBuilder();
            if (current.Count == 0)
            {
                await ReplyAsync("``The RSS filter is empty.``").ConfigureAwait(false);
                return;
            }
            else
            {
                sb.Append("```Current Filters:```");
                sb.AppendLine();
                foreach (var filter in current)
                {
                    sb.Append("``" + filter + "``");
                    sb.AppendLine();
                }
            }
            await ReplyAsync(sb.ToString()).ConfigureAwait(false);
        }

        [Command("rssremove")]
        [Summary("Removes specified rss filter(s).")]
        public async Task RSSRemove([Remainder] string query)
        {
            string[] items = query.Split(new[] { "|" }, 2, StringSplitOptions.RemoveEmptyEntries);
            string[] filters = items[0].TrimEnd(' ').Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            List<string> nonexisting = new List<string>();
            List<string> temp = new List<string>();

            string mode;
            string resource;
            if (filters[0].Substring(0, 2).Equals("a "))
            {
                resource = Path.GetFullPath(Resources.anime_filters, Extensions.config_values.root_path);
                mode = filters[0].Substring(0, 1);
                filters[0] = filters[0].Substring(2);
            }
            else if (filters[0].Substring(0, 2).Equals("m "))
            {
                resource = Path.GetFullPath(Resources.manga_filters, Extensions.config_values.root_path);
                mode = filters[0].Substring(0, 1);
                filters[0] = filters[0].Substring(2);
            }
            else
            {
                resource = Path.GetFullPath(Resources.anime_filters, Extensions.config_values.root_path);
                mode = "a";
            }

            List<string> current = Extensions.readFile(resource);
            foreach (string unparsed in filters)
            {
                int flag = 0;
                string input = unparsed.TrimStart(' ').ToLower();
                if (input.Length > 2)
                {
                    foreach (string filter in current)
                    {
                        if (filter.Contains(input))
                        {
                            temp.Add(filter);
                            flag = 1;
                        }
                    }
                }
                if (flag == 0)
                {
                    nonexisting.Add(input);
                }
            }
            foreach (string s in temp)
            {
                current.Remove(s);
            }
            if (nonexisting.Count < filters.Length)
            {
                if (nonexisting.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var wrong in nonexisting)
                    {
                        sb.Append(", " + wrong);
                    }
                    await ReplyAsync("``" + sb.ToString().Substring(2) + (nonexisting.Count == 1 ? " isn't " : " aren't ") + "registered.``").ConfigureAwait(false);
                }
                if (mode == "a")
                {
                    if (RSSTorrentUtility.qBitTorrentRemoveFilter(filters))
                    {
                        Extensions.writeLinesToFile(current, resource);
                        RSSfeed.RSSRefresh();
                        await ReplyAsync("``The RSS filter has been updated.``").ConfigureAwait(false);
                        RSSTorrentUtility.qBittorrentRestart();
                    }
                    else
                        await ReplyAsync("``There was an issue updating the qBitTorrent autodownload config.``").ConfigureAwait(false);
                }
                else
                {
                    Extensions.writeLinesToFile(current, resource);
                    RSSfeed.RSSRefresh();
                    await ReplyAsync("``The RSS filter has been updated.``").ConfigureAwait(false);
                }
            }
            else
            {
                await ReplyAsync("``None of the filters you tried to remove is registered.``").ConfigureAwait(false);
            }
            if (items.Length == 2 && items[1].TrimStart(' ').Equals("s"))
            {
                await RSSShow(mode);
            }
        }

        [Command("rssremoveall")]
        [Summary("Removes all rss filters.")]
        public async Task RSSRemoveall([Remainder] string query = "a")
        {
            string resource;
            if (query.Equals("a"))
            {
                resource = Path.GetFullPath(Resources.anime_filters, Extensions.config_values.root_path);
                List<string> filters = Extensions.readFile(resource);
                if (!RSSTorrentUtility.qBitTorrentRemoveFilter(filters.ToArray()))
                    await ReplyAsync("``There was an issue updating the qBitTorrent autodownload config. Please check it manually.``").ConfigureAwait(false);
            }
            else if (query.Equals("m"))
            {
                resource = Path.GetFullPath(Resources.manga_filters, Extensions.config_values.root_path);
            }
            else
            {
                resource = Path.GetFullPath(Resources.anime_filters, Extensions.config_values.root_path);
            }

            List<string> temp = new List<string>();
            Extensions.writeLinesToFile(temp, resource);
            RSSfeed.RSSRefresh();
            await ReplyAsync("``The RSS filter is now empty.``").ConfigureAwait(false);
            RSSTorrentUtility.qBittorrentRestart();
        }

        [Command("rssaddgeneralsource")]
        [Summary("Adds a new unfiltered source.")]
        public async Task RSSAddGeneralSource([Remainder] string query)
        {
            string input = query.TrimStart(' ').TrimEnd(' ');
            if (!input.Contains(" "))
            {
                List<string> current = Extensions.readFile(Path.GetFullPath(Resources.general_sources, Extensions.config_values.root_path));
                if (!current.Contains(input))
                {
                    current.Add(input);
                    Extensions.writeLinesToFile(current, Path.GetFullPath(Resources.general_sources, Extensions.config_values.root_path));
                    RSSfeed.RSSRefresh();
                    await ReplyAsync("``A new RSS source has been added.``").ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync("``The source you tried to add is already registered.``").ConfigureAwait(false);
                }
            }
            else
            {
                await ReplyAsync("``The source you tried to add is in an invalid format. (Make sure it has no spaces.)``").ConfigureAwait(false);
            }

        }

        [Command("rssaddsource")]
        [Summary("Adds a new rss source (for torrents).")]
        public async Task RSSAddTorrentSource([Remainder] string query)
        {
            string input = query.TrimStart(' ').TrimEnd(' ');
            if (!input.Contains(" "))
            {
                List<string> current = Extensions.readFile(Path.GetFullPath(Resources.anime_sources, Extensions.config_values.root_path));
                if (!current.Contains(input))
                {
                    current.Add(input);
                    if (RSSTorrentUtility.qBitTorrentAddSource(input))
                    {
                        Extensions.writeLinesToFile(current, Path.GetFullPath(Resources.anime_sources, Extensions.config_values.root_path));
                        RSSfeed.RSSRefresh();
                        await ReplyAsync("``A new RSS source has been added.``").ConfigureAwait(false);
                        RSSTorrentUtility.qBittorrentRestart();
                    }
                    else
                        await ReplyAsync("``There was an issue updating the qBitTorrent autodownload config.``").ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync("``The source you tried to add is already registered.``").ConfigureAwait(false);
                }
            }
            else
            {
                await ReplyAsync("``The source you tried to add is in an invalid format. (Make sure it has no spaces.)``").ConfigureAwait(false);
            }

        }

        [Command("rssremovesource")]
        [Summary("Removes a rss source.")]
        public async Task RSSRemoveSource([Remainder] string query)
        {
            string input = query.TrimStart(' ').TrimEnd(' ');
            if (!input.Contains(" "))
            {
                List<string> general = Extensions.readFile(Path.GetFullPath(Resources.general_sources, Extensions.config_values.root_path));
                List<string> manga = Extensions.readFile(Path.GetFullPath(Resources.manga_sources, Extensions.config_values.root_path));
                List<string> torrents = Extensions.readFile(Path.GetFullPath(Resources.anime_sources, Extensions.config_values.root_path));
                if (manga.Remove(input) || torrents.Remove(input) || general.Remove(input))
                {
                    Extensions.writeLinesToFile(general, Path.GetFullPath(Resources.general_sources, Extensions.config_values.root_path));
                    Extensions.writeLinesToFile(manga, Path.GetFullPath(Resources.manga_sources, Extensions.config_values.root_path));
                    if (RSSTorrentUtility.qBitTorrentRemoveSource(input))
                    {
                        Extensions.writeLinesToFile(torrents, Path.GetFullPath(Resources.anime_sources, Extensions.config_values.root_path));
                        RSSfeed.RSSRefresh();
                        await ReplyAsync("``The specified RSS source has been removed.``").ConfigureAwait(false);
                        RSSTorrentUtility.qBittorrentRestart();
                    }
                    else
                        await ReplyAsync("``There was an issue updating the qBitTorrent autodownload config.``").ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync("``The source you tried to remove isn't registered.``").ConfigureAwait(false);
                }
            }
            else
            {
                await ReplyAsync("``The source you tried to remove is in an invalid format. (Make sure it has no spaces.)``").ConfigureAwait(false);
            }

        }

        [Command("rssremoveallsources")]
        [Summary("Removes all rss sources.")]
        public async Task RSSRemoveAllSources()
        {
            List<string> torrents = Extensions.readFile(Path.GetFullPath(Resources.anime_sources, Extensions.config_values.root_path));
            foreach (string source in torrents)
            {
                if (!RSSTorrentUtility.qBitTorrentRemoveSource(source))
                {
                    await ReplyAsync("``There was an issue updating the qBitTorrent autodownload config. Please check it manually.``").ConfigureAwait(false);
                    return;
                }
            }

            List<string> temp = new List<string>();
            Extensions.writeLinesToFile(temp, Path.GetFullPath(Resources.general_sources, Extensions.config_values.root_path));
            Extensions.writeLinesToFile(temp, Path.GetFullPath(Resources.manga_sources, Extensions.config_values.root_path));
            Extensions.writeLinesToFile(temp, Path.GetFullPath(Resources.anime_sources, Extensions.config_values.root_path));
            RSSfeed.RSSRefresh();

            await ReplyAsync("``All the RSS sources were removed.``").ConfigureAwait(false);
            RSSTorrentUtility.qBittorrentRestart();
        }

        [Command("rssshowsources")]
        [Summary("Shows list of sources in the rss filter.")]
        public async Task RSSShowSources()
        {
            List<string> current = Extensions.readFile(Path.GetFullPath(Resources.manga_sources, Extensions.config_values.root_path));
            current.AddRange(Extensions.readFile(Path.GetFullPath(Resources.anime_sources, Extensions.config_values.root_path)));
            current.AddRange(Extensions.readFile(Path.GetFullPath(Resources.general_sources, Extensions.config_values.root_path)));

            StringBuilder sb = new StringBuilder();
            if (current.Count == 0)
            {
                await ReplyAsync("``No RSS sources are defined.``").ConfigureAwait(false);
                return;
            }
            else
            {
                sb.Append("```Current Sources:```");
                sb.AppendLine();
                foreach (var source in current)
                {
                    sb.Append("``" + source + "``");
                    sb.AppendLine();
                }
            }
            await ReplyAsync(sb.ToString()).ConfigureAwait(false);
        }
    }
}