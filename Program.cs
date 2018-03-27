using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Bardiche.Modules;
using Bardiche.Classes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Bardiche.Properties;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;
using Bardiche.JSONModels;

namespace Bardiche
{
    public class Bardiche
    {
        public static void Main(string[] args)
        {
            int f = 1;
            do
            {
                try
                {
                    new Bardiche().Start().GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Console.Write(e.StackTrace);
                    f = 0;
                }
            } while (f == 0);
        }

        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;
        private RSSfeed feed;
        /*
        AdministrationModule admod = new AdministrationModule();
        SearchesModule smod = new SearchesModule();
        UtilityModule umod = new UtilityModule();
        RSSControlModule rssmod = new RSSControlModule();*/

        private static DateTime startTime = DateTime.Now;

        public async Task Start()
        {
            client = new DiscordSocketClient();
            commands = new CommandService();

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .BuildServiceProvider();

            await InstallCommandsAsync();

            Extensions.config_values = await Task.Run(() => JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText(Resources.config)));

            try
            {
                await client.LoginAsync(TokenType.Bot, Extensions.config_values.bot_token);
                await client.StartAsync();
            }
            catch
            {
                Console.WriteLine("Invalid bot token. Please edit a valid one into the config.json file located under Resources.");
                await Task.Delay(15000);
                return;
            }


            feed = new RSSfeed();

            Extensions.setUpdate(startTime);
            //Extensions.RefreshDirectories();
            Extensions.ReminderHandler(client);

            Console.WriteLine("SET UP");

            ITextChannel channel = null;
            try {
                int safetyCheck = int.MaxValue;
                while (channel == null)
                {
                    channel = (ITextChannel)await ((IDiscordClient)client).GetChannelAsync(Extensions.config_values.rss_channel_id);
                    safetyCheck--;
                    if (safetyCheck == 0)
                    {
                        Console.WriteLine("Channel for RSS feed is taking too long to retrieve.\nPlease make sure your rss channel id field in config.json is set correctly.");
                    }
                }
                Console.WriteLine("RSS service SET UP.");
                await feed.ReadRSS(channel);
            } catch { Console.WriteLine("Channel for RSS feed not configured. Cannot run RSS service.\nPlease set a channel in the config.json file."); }

            await Task.Delay(-1);
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommandAsync;
            // Discover all of the commands in this assembly and load them.
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage parameterMessage)
        {
            var message = parameterMessage as SocketUserMessage;

            if ((message == null) || new Regex("(!+$)|(!+\\s)").IsMatch(message.ToString())) return;

            int argPos = 0;

            IReadOnlyCollection<IAttachment> attachments = message.Attachments;
            foreach (IAttachment a in attachments)
            {
                if (a.Filename.Substring(a.Filename.Length - 4, 4).Equals(".bmp"))
                {
                    if (Extensions.UrltoJPEG(a.Url))
                    {
                        await message.DeleteAsync();
                        await message.Channel.SendFileAsync(Resources.temp, $"``Sent by {message.Author.Username}``");
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("``Conversion into jpeg failed.``");
                    }
                }
            }

            if (message.ToString().Contains(".bmp"))
            {
                char[] separators = { ' ', '\n' };
                string[] s_message = message.ToString().Split(separators);
                foreach (string s in s_message)
                {
                    if ((s.Length > 3) && (s.Substring(s.Length - 4, 4).Equals(".bmp")))
                    {
                        if (Extensions.UrltoJPEG(s))
                        {
                            await message.Channel.SendFileAsync(Resources.temp);
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("``Conversion into jpeg failed.``");
                        }
                    }
                }
            }

            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasCharPrefix('!', ref argPos) || message.ToString().Substring(0, 8).Equals("Bardiche"))) return;
            var context = new CommandContext(client, message);
            var result = await commands.ExecuteAsync(context, argPos, services);

            if (!result.IsSuccess)
                await message.Channel.SendMessageAsync($"``{result.ErrorReason}``");
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

    }
}