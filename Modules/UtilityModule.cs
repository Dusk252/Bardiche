using Discord;
using Discord.Commands;
using System.Text;
using System.Threading.Tasks;
using Bardiche.Classes;
using System;
using System.IO;
using Bardiche.Properties;

namespace Bardiche.Modules
{
    public class UtilityModule : ModuleBase
    {

        [Command("help")]
        [Alias("man")]
        [Summary("Provides a list of help commands.")]
        public async Task Help()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("```Call Commands```");
            sb.AppendLine("`Bardiche*`     `Returns \"YES SIR!\".`");
            string call = sb.ToString();

            sb = new StringBuilder();
            sb.AppendLine("\n```Utility Commands```");
            sb.AppendLine("`!rng <n1> [<n2>]`     `Generates a number from 1 to n1 if no n2 specified or from n1 to n2 if specified.`");
            sb.AppendLine("`(!choose | !c) <op1>[<separator><op2>]...`     `Chooses an option from the list provided. Comma and space are valid separators.`");
            sb.AppendLine("`(!reminder | !rm) <msg>:<time>`     `Repeats the input message after the specified time has elapsed. Valid expressions for <time> are: <min>m, <hours>h, <hours>h<min>m`");
            string utility = sb.ToString();

            sb = new StringBuilder();
            sb.AppendLine("\n```Search Commands```");
            sb.AppendLine("`(!anime | !ani | !a) <query> [<year>]`     `Queries anilist for [query] and shows the first anime result. Optional parameter <year> taken.`");
            sb.AppendLine("`(!manga | !m) <query> [<type>]`     `Queries anilist for <query> and shows the first manga result. Optional parameter <type> taken.`");
            sb.AppendLine("`(!vndb | !vn) <query>`     `Queries vndb for <query> and shows the first result.`");
            string search = sb.ToString();

            /*sb = new StringBuilder();
            sb.AppendLine("\n```Miscellaneous Commands```");
            sb.AppendLine("`!i <series>`     `Posts a random pic from the specified series folder.`");
            string misc = sb.ToString();*/

            sb = new StringBuilder();
            sb.AppendLine("\n```Info Commands```");
            sb.AppendLine("`(!channeltopic | !ct)`     `Sends current channel's topic as a message.`");
            //sb.AppendLine("`!uptime`     `Says for how long the bot has been up.`");
            //sb.AppendLine("`!savechat <n>`     `Saves the last <n> messages from the chat and sends to user as file.`");
            string info = sb.ToString();

            sb = new StringBuilder();
            sb.AppendLine("\n```RSS Commands```");
            sb.AppendLine("`!rssadd <show>[,<show>]...[|s]`     `Adds <show>s to the filters. Show must have no spaces. |s calls rssshow.`");
            sb.AppendLine("`!rssremove <show>[,<show>]...[|s]`     `Removes filter for <show>s. |s calls rssshow.`");
            sb.AppendLine("`!rssremoveall`     `Removes all anime rss filters.`");
            sb.AppendLine("`!rssshow`     `Prints current filters.`");
            sb.AppendLine("`!rssaddsource (a |g ) <source>`     `Adds <source> to the current list of torrent sources. Sources must have no spaces. a for the filtered service, g for the general ones.`");
            sb.AppendLine("`!rssremovesource <source>`     `Removes <source> from the current list of sources.`");
            sb.AppendLine("`!rssremoveallsources`     `Removes all rss sources.`");
            sb.AppendLine("`!rssshowsources`     `Prints current sources.`");
            string rss = sb.ToString();

            sb = new StringBuilder();
            sb.AppendLine("\n```Manage Channel```");
            sb.AppendLine("`!del <n>     Deletes the last <n> messages of the user who called this command.`");
            sb.AppendLine("`!delbot <n>     Deletes all bot messages. (admin only)`");
            sb.AppendLine("`!prune <n>     Deletes the last <n> messages in the chat. (admin only)`");
            string manage = sb.ToString();

            await ReplyAsync(call + utility + search /*+ misc*/).ConfigureAwait(false);
            await ReplyAsync(info + rss + manage).ConfigureAwait(false);
        }

        [Command("Bardiche")]
        [Alias("Bardiche!", "Bardiche,", "Bardiche.")]
        [Summary("Returns \"YES SIR!\".")]
        public async Task Name(params string[] str)
        {
            await ReplyAsync("``" + "YES SIR!" + "``").ConfigureAwait(false);
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
                await ReplyAsync("``" + "Please insert one integer or two integers separated by a comma as parameters." + "``").ConfigureAwait(false);
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
            int hours = 0;
            int min = 0;
            try
            {
                string[] separators = { " : ", ":" };
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
                    //string user = Context.User.ToString();
                    DateTime rmtime = DateTime.Now + new TimeSpan(hours, min, 0);
                    StringBuilder sb = new StringBuilder();
                    sb.Append(rmtime.ToBinary() + ";");
                    sb.Append("``" + msg + "``;");
                    sb.Append(Context.User.Id + ";");
                    sb.Append(Context.Channel.Id);
                    sb.AppendLine();
                    File.AppendAllText(Resources.reminders, sb.ToString());
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

 /*       [Command("i")]
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
        }*/

        //not fully working
        [Command("uptime")]
        [Summary("Sends a message indicating for how long the bot has been on.")]
        public async Task Uptime()
        {
            TimeSpan span = DateTime.Now - Extensions.startTime;
            await ReplyAsync("``Up since " + Extensions.startTime + ".\n" + Extensions.TimeToString(span) + " have passed.``").ConfigureAwait(false);
        }
    }
}
