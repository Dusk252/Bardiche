using Bardiche.Classes;
using Bardiche.Properties;
using Discord.Commands;
using System;
using System.Collections.Generic;
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

            string resource = Resources.rss_filters;

            List<string> current = Extensions.readRSS(resource);
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
                Extensions.writeRSS(current, resource);
                RSSfeed.RSSRefresh();
                await ReplyAsync("``The RSS filter has been updated.``").ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync("``All the filters you tried to add already exist.``").ConfigureAwait(false);
            }
            if (items.Length == 2 && items[1].TrimStart(' ').Equals("s"))
            {
                await RSSShow();
            }
        }

        [Command("rssshow")]
        [Summary("Shows list of shows in the rss filter.")]
        public async Task RSSShow()
        {
            List<string> current;
            current = Extensions.readRSS(Resources.rss_filters);
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

            string resource = Resources.rss_filters;

            List<string> current = Extensions.readRSS(resource);
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
                Extensions.writeRSS(current, resource);
                RSSfeed.RSSRefresh();
                await ReplyAsync("``The RSS filter has been updated.``").ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync("``None of the filters you tried to add is registered.``").ConfigureAwait(false);
            }
            if (items.Length == 2 && items[1].TrimStart(' ').Equals("s"))
            {
                await RSSShow();
            }
        }

        [Command("rssremoveall")]
        [Summary("Removes all rss filters.")]
        public async Task RSSRemoveall([Remainder] string query = "a")
        {
            string resource = Resources.rss_filters;

            List<string> temp = new List<string>();
            Extensions.writeRSS(temp, resource);
            RSSfeed.RSSRefresh();
            await ReplyAsync("``The RSS filter is now empty.``").ConfigureAwait(false);
        }

        [Command("rssaddsource")]
        [Summary("Adds a new rss source.")]
        public async Task RSSAddTorrentSource([Remainder] string query)
        {
            string input = query.TrimStart(' ').TrimEnd(' ');
            string resource = Resources.rss_filters;
            if (input.Contains(" "))
            {
                if (input.Substring(0, 2).Equals("a "))
                {
                    resource = Resources.filtered_sources;
                }
                else if (input.Substring(0, 2).Equals("g "))
                {
                    resource = Resources.general_sources;
                }
                input = input.Substring(2);
            }
            if (!input.Contains(" "))
            {
                List<string> current = Extensions.readRSS(resource);
                if (!current.Contains(input))
                {
                    current.Add(input);
                    Extensions.writeRSS(current, resource);
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

        [Command("rssremovesource")]
        [Summary("Removes a rss source.")]
        public async Task RSSRemoveSource([Remainder] string query)
        {
            string input = query.TrimStart(' ').TrimEnd(' ');
            if (!input.Contains(" "))
            {
                List<string> general = Extensions.readRSS(Resources.general_sources);
                List<string> torrents = Extensions.readRSS(Resources.filtered_sources);
                if (general.Remove(input)||torrents.Remove(input))
                {
                    Extensions.writeRSS(general, Resources.general_sources);
                    Extensions.writeRSS(torrents, Resources.filtered_sources);
                    RSSfeed.RSSRefresh();
                    await ReplyAsync("``The specified RSS source has been removed.``").ConfigureAwait(false);
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
        [Summary("Removes all rss filters.")]
        public async Task RSSRemoveAllSources()
        {
            List<string> temp = new List<string>();
            Extensions.writeRSS(temp, Resources.general_sources);
            Extensions.writeRSS(temp, Resources.filtered_sources);
            RSSfeed.RSSRefresh();
            await ReplyAsync("``All the RSS sources were removed.``").ConfigureAwait(false);
        }

        [Command("rssshowsources")]
        [Summary("Shows list of shows in the rss filter.")]
        public async Task RSSShowSources()
        {
            List<string> current = Extensions.readRSS(Resources.general_sources);
            current.AddRange(Extensions.readRSS(Resources.filtered_sources));
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
