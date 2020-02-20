using Discord;
using Discord.Commands;
using System.Text;
using System.Threading.Tasks;
using Bardiche.Classes;
using System;
using System.IO;
using Bardiche.Properties;
using System.Globalization;

namespace Bardiche.Modules
{
    public class UtilityModule : ModuleBase
    {

        [Command("help")]
        [Alias("man")]
        [Summary("Provides a list of help commands.")]
        public async Task Help([Remainder] string input = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("```Call Commands```");
            sb.AppendLine("`Bardiche*`     `Returns \"YES SIR!\".`");
            sb.AppendLine("`Sure*`     `Returns a certain Katanagatari quote.`");
            sb.AppendLine("`!cheerio`     `A popular phrase in the Satsuma domain in Kyushu used to raise one's spirits.`");
            string call = sb.ToString();

            sb = new StringBuilder();
            sb.AppendLine("\n```Utility Commands```");
            sb.AppendLine("`!rng <n1> [<n2>]`     `Generates a number from 1 to n1 if no n2 specified or from n1 to n2 if specified.`");
            sb.AppendLine("`(!choose | !c) <op1>[<separator><op2>]...`     `Chooses an option from the list provided. Comma and space are valid separators.`");
            sb.AppendLine("`!listshuffle | !ls <entry1>[<separator><entry2>]...`     `Returns an input list shuffled. New line and comma are valid separators.`");
            sb.AppendLine("`(!reminder | !rm) <msg>$<datetime>`     `Repeats the input message on the specified date and time. Supported formats are yyyy-MM-dd, yyyy-MM-dd hh:mm, yyyy/MM/dd, yyyy/MM/dd hh:mm.`");
            sb.AppendLine("`(!alert) <msg>$<time>`     `Repeats the input message after the specified time has elapsed. Valid expressions for <time> are: <min>m, <hours>h, <hours>h<min>m`");
            string utility = sb.ToString();

            sb = new StringBuilder();
            sb.AppendLine("\n```Search Commands```");
            sb.AppendLine("`(!anime | !ani | !a) <query> [<year>]`     `Queries anilist for [query] and shows the first anime result. Optional parameter <year> taken.`");
            sb.AppendLine("`(!manga | !m) <query> [<type>]`     `Queries anilist for <query> and shows the first manga result. Optional parameter <type> taken.`");
            sb.AppendLine("`(!vndb | !vn) <query>`     `Queries vndb for <query> and shows the first result.`");
            string search = sb.ToString();

            sb = new StringBuilder();
            sb.AppendLine("\n```Miscellaneous Commands```");
            sb.AppendLine("`!i <series>`     `Posts a random pic from the specified series folder. Supported series are: dal, dr, fate, flifla, geah, gg, ikimashou, katana, noragami, ph, pmmm, rezero, tutu, wide`");
            sb.AppendLine("`!glare`     `Estelle glares at your opponent.`");
            string misc = sb.ToString();

            sb = new StringBuilder();
            sb.AppendLine("\n```Info Commands```");
            //sb.AppendLine("`!serverinfo | !sinfo`     `Shows info about the server.`");
            //sb.AppendLine("`!channelinfo | !cinfo`     `Shows info about the channel.`");
            //sb.AppendLine("`!userinfo | !uinfo`     `Shows info about the user.`");
            sb.AppendLine("`!channeltopic | !ct`     `Sends current channel's topic as a message.`");
            //sb.AppendLine("`!savechat [n]`     `Saves the last [n] messages from the chat and sends to user as file.`");
            string info = sb.ToString();

            sb = new StringBuilder();
            sb.AppendLine("\n```RSS Commands```");
            sb.AppendLine("`!rssadd [a |m ]<series>[,<series>]...[|s]`     `Adds <series> to the filters. Show must have no spaces. |s calls rssshow.`");
            sb.AppendLine("`!rssremove  [a |m ]<series>[,<series>]...[|s]`     `Removes filter for <series>. |s calls rssshow.`");
            sb.AppendLine("`!rssremoveall`     `Removes all anime rss filters.`");
            sb.AppendLine("`!rssshow`     `Prints current filters.`");
            sb.AppendLine("`!rssaddsource [a |m ]<source>`     `Adds <source> to the current list of torrent sources. Sources must have no spaces. a for anime, m for manga.`");
            sb.AppendLine("`!rssaddgeneralsource <source>`     `Adds unfiltered <source>.`");
            sb.AppendLine("`!rssremovesource <source>`     `Removes <source> from the current list of sources.`");
            sb.AppendLine("`!rssremoveallsources`     `Removes all rss sources.`");
            sb.AppendLine("`!rssshowsources`     `Prints current sources.`");
            string rss = sb.ToString();

            sb = new StringBuilder();
            sb.AppendLine("\n```Manage Channel```");
            //sb.AppendLine("`!setchannelname | !schn [name]`     `Sets [name] as the name of the current channel.`");
            //sb.AppendLine("`!settopic | !st [topic]`     `Sets [topic] as the topic of the current channel.`");
            sb.AppendLine("`!del <n>     Deletes the last <n> messages of the user who called this command.`");
            sb.AppendLine("`!delbot <n>     Deletes all bot messages. (admin only)`");
            sb.AppendLine("`!prune <n>     Deletes the last <n> messages in the chat. (admin only)`");
            string manage = sb.ToString();

            input = input.Trim();
            switch (input) {
                case "rss":
                    await ReplyAsync(rss).ConfigureAwait(false);
                    break;
                case "call":
                    await ReplyAsync(call).ConfigureAwait(false);
                    break;
                case "utility":
                    await ReplyAsync(utility).ConfigureAwait(false);
                    break;
                case "search":
                    await ReplyAsync(search).ConfigureAwait(false);
                    break;
                case "misc":
                    await ReplyAsync(misc).ConfigureAwait(false);
                    break;
                case "info":
                    await ReplyAsync(info).ConfigureAwait(false);
                    break;
                case "manage":
                    await ReplyAsync(manage).ConfigureAwait(false);
                    break;
                case "":
                    await ReplyAsync(call + utility + search + misc).ConfigureAwait(false);
                    await ReplyAsync(info + rss + manage).ConfigureAwait(false);
                    break;
                default:
                    await ReplyAsync("``" + "There is no " + input + " command section in this help document." + "``").ConfigureAwait(false);
                    break;
            }
        }

        [Command("Bardiche")]
        [Alias("Bardiche!", "Bardiche,", "Bardiche.")]
        [Summary("Returns \"YES SIR!\".")]
        public async Task Name(params string[] str)
        {
            await ReplyAsync("``" + "YES SIR!" + "``").ConfigureAwait(false);
        }

        [Command("cheerio")]
        [Summary("Cheerio!")]
        public async Task Cheerio()
        {
            await ReplyAsync("``" + "Cheerio!" + "``").ConfigureAwait(false);
        }

        [Command("Sure!")]
        [Alias("Sure", "Sure,", "Sure.")]
        [Summary("Returns a certain Katanagatari quote.")]
        public async Task Sure(params string[] str)
        {
            await ReplyAsync("``" + "However, by that point you'll have been torn into pieces." + "``").ConfigureAwait(false);
        }

        [Command("glare")]
        [Summary("Returns a Estelle pic.")]
        public async Task Glare()
        {
            await Context.Channel.SendFileAsync(Path.GetFullPath(Resources.estelle, Extensions.config_values.root_path)).ConfigureAwait(false);
        }

        [Command("channel_topic")]
        [Alias("ct")]
        [Summary("Sends current channel's topic as a message.")]
        public async Task ChannelTopic([Remainder]ITextChannel channel = null)
        {
            if (channel == null)
                channel = (ITextChannel)Context.Channel;

            var topic = channel.Topic;
            if (string.IsNullOrWhiteSpace(topic))
                await ReplyAsync("``" + "No topic set." + "``").ConfigureAwait(false);
            else
                await Context.Channel.SendMessageAsync("``" + topic + "``").ConfigureAwait(false);
        }

        [Command("countdown")]
        [Alias("ctd")]
        [Summary("Starts a countdown.")]
        public async Task Countdown([Remainder]ITextChannel channel = null)
        {
            for (int a = 3; a > 0; a--)
            {
                await ReplyAsync("``" + a + "``\n").ConfigureAwait(false);
                System.Threading.Thread.Sleep(1000);
            }
            await ReplyAsync("``" + "GO" + "``").ConfigureAwait(false);
        }

        [Command("rng")]
        [Summary("Returns a random integer in a range up to the number specified or between the numbers specified.")]
        public async Task Rng([Remainder] string input)
        {
            string[] separators = { ", " };
            string[] str = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            int n1, n2;

            try
            {
                n1 = int.Parse(str[0]);
                try
                {
                    n2 = int.Parse(str[1]);
                }
                catch
                {
                    n2 = 1;
                }
            }
            catch
            {
                await ReplyAsync("``" + "Please insert one or two integers as parameters." + "``").ConfigureAwait(false);
                return;
            }

            await ReplyAsync("``" + Extensions.RNG(n1, n2) + "``").ConfigureAwait(false);
        }

        [Command("choose")]
        [Alias("c")]
        [Summary("Returns a random item from a list of things.")]
        public async Task Choose([Remainder] string input)
        {
            string[] separators = { " ", ",", ", " };
            string[] items = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            string result = items[Extensions.RNG(0, items.Length - 1)];

            await ReplyAsync("``" + result + "``").ConfigureAwait(false);
        }

        [Command("reminder")]
        [Alias("rm")]
        [Summary("Reminds user after specified time.")]
        public async Task Reminder([Remainder] string input)
        {
            try
            {
                string[] separators = { " $ ", "$" };
                string[] dateTimeFormats = { "yyyy-MM-dd", "yyyy-MM-dd HH:mm", "yyyy/MM/dd", "yyyy/MM/dd HH:mm" };
                string[] temp = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                if (temp.Length != 2)
                    await ReplyAsync("``" + "Wrong message format." + "``").ConfigureAwait(false);
                else
                {
                    string msg = temp[0];

                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        DateTime rmtime;
                        TimeSpan diff = Extensions.config_values.time_zones.ContainsKey(Context.User.Id) ? Extensions.config_values.time_zones[Context.User.Id] : TimeSpan.Zero;
                        
                        if (!DateTime.TryParseExact(temp[1], dateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out rmtime))
                            await ReplyAsync("``" + "Invalid DateTime format." + "``").ConfigureAwait(false);
                        else
                        {
                            DateTime finalTime = rmtime - diff;
                            if (finalTime < DateTime.Now)
                                await ReplyAsync("``" + "Unfortunately I am not equipped with a time travel function." + "``").ConfigureAwait(false);
                            else
                            {
                                await ReplyAsync("``Reminder set to " + rmtime.ToString("yyyy-MM-dd") + " at " + rmtime.TimeOfDay.ToString(@"hh\:mm")
                                    + " UTC" + (diff < TimeSpan.Zero ? "-" : "+") + diff.ToString(@"hh\:mm") + ".``").ConfigureAwait(false);
                                string user = Context.User.ToString();

                                StringBuilder sb = new StringBuilder();
                                rmtime = rmtime + diff;
                                sb.Append(finalTime.ToBinary() + ";");
                                sb.Append("``" + msg + "``;");
                                sb.Append(Context.User.Id + ";");
                                sb.Append(Context.Channel.Id);
                                sb.AppendLine();
                                File.AppendAllText(Path.GetFullPath(Resources.reminders, Extensions.config_values.root_path), sb.ToString());
                            }
                        }
                    }
                    else
                    {
                        await ReplyAsync("``" + "You didn't insert a message." + "``").ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                await ReplyAsync("``" + "An error ocurred." + "``").ConfigureAwait(false);
            }
        }

        [Command("alert")]
        [Summary("Reminds user after specified time.")]
        public async Task Alert([Remainder] string input)
        {
            int hours = 0;
            int min = 0;
            try
            {
                string[] separators = { " $ ", "$" };
                string[] temp = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                if (temp.Length > 2) throw new Exception();

                string[] separatorst = { "h", "m" };
                string t_str = temp[1];
                string msg = temp[0];
                string[] time = t_str.Split(separatorst, StringSplitOptions.RemoveEmptyEntries);
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    if (time.Length > 2 || !(t_str.EndsWith("h") || t_str.EndsWith("m")))
                    {
                        throw new Exception();
                    }
                    if (t_str.Contains("h"))
                    {
                        if (t_str.Contains("m"))
                        {
                            hours = Int32.Parse(time[0]);
                            min = Int32.Parse(time[1]);
                            await ReplyAsync("``Will remind you of that in " + hours + (hours == 1 ? " hour" : " hours") + " and " + min + (min == 1 ? " minute" : " minutes") + ".``").ConfigureAwait(false);
                        }
                        else
                        {
                            hours = Int32.Parse(time[0]);
                            await ReplyAsync("``Will remind you of that in " + hours + (hours == 1 ? " hour" : " hours") + ".``").ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        if (t_str.Contains("m"))
                        {
                            min = Int32.Parse(time[0]);
                            await ReplyAsync("``Will remind you of that in " + min + (min == 1 ? " minute" : " minutes") + ".``").ConfigureAwait(false);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    string user = Context.User.ToString();
                    DateTime rmtime = DateTime.Now + new TimeSpan(hours, min, 0);
                    StringBuilder sb = new StringBuilder();
                    sb.Append(rmtime.ToBinary() + ";");
                    sb.Append("``" + msg + "``;");
                    sb.Append(Context.User.Id + ";");
                    sb.Append(Context.Channel.Id);
                    sb.AppendLine();
                    File.AppendAllText(Path.GetFullPath(Resources.reminders, Extensions.config_values.root_path), sb.ToString());
                }
                else
                {
                    await ReplyAsync("``" + "You didn't insert a message." + "``").ConfigureAwait(false);
                }
            }
            catch
            {
                await ReplyAsync("``" + "Invalid time." + "``").ConfigureAwait(false);
            }
        }

        [Command("i")]
        [Summary("Posts an image of the specified series.")]
        public async Task I([Remainder] string input)
        {
            try
            {
                input.TrimStart(' ');
                string file = Extensions.directories[input][Extensions.RNG(0, Extensions.directories[input].Length - 1)];
                await Context.Channel.SendFileAsync(file).ConfigureAwait(false);
            }
            catch
            {
                await ReplyAsync("``The series named is not supported.``").ConfigureAwait(false);
            }
        }

        [Command("uptime")]
        [Summary("Sends a message indicating for how long the bot has been on.")]
        public async Task Uptime()
        {
            TimeSpan span = DateTime.Now - Extensions.startTime;
            await ReplyAsync("``Up since " + Extensions.startTime + ".\n" + Extensions.TimeToString(span) + " have passed.``").ConfigureAwait(false);
        }
    }
}
