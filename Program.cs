using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Bardiche.Classes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Bardiche.Properties;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bardiche.JSONModels;
using System.IO;
using Newtonsoft.Json;

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
                    continue;
                }
            } while (f == 0);
        }

        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;
        private RSSfeed feed;
        private FGONews fgoFeed;

        /*AdministrationModule admod = new AdministrationModule();
        SearchesModule smod = new SearchesModule();
        UtilityModule umod = new UtilityModule();
        RSSControlModule rssmod = new RSSControlModule();*/

        private static DateTime startTime = DateTime.Now;
        private static string rootPath = @"C:\repos\Bardiche_v2.1";

        public async Task Start()
        {
            client = new DiscordSocketClient();
            commands = new CommandService();

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .BuildServiceProvider();

            await InstallCommandsAsync();

            Extensions.config_values = await Task.Run(() => JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText(Path.GetFullPath(Resources.config, rootPath))));
            Extensions.config_values.root_path = rootPath;

            await client.LoginAsync(TokenType.Bot, Extensions.config_values.bot_token);
            await client.StartAsync();

            feed = new RSSfeed();
            fgoFeed = new FGONews();
            Extensions.setUpdate(startTime);
            //Extensions.RefreshDirectories();
            Extensions.ReminderHandler(client);
         
            feed.ReadRSS();
            fgoFeed.readNews();

            Console.WriteLine("SET UP");

            await Task.Delay(-1);
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommandAsync;
            // Discover all of the commands in this assembly and load them.
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
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
                        await message.Channel.SendFileAsync(Path.GetFullPath(Resources.temp, Extensions.config_values.root_path), $"``Sent by {message.Author.Username}``");
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
                            await message.Channel.SendFileAsync(Path.GetFullPath(Resources.temp, Extensions.config_values.root_path));
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("``Conversion into jpeg failed.``");
                        }
                    }
                }
            }

            //if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasCharPrefix('!', ref argPos) || (message.ToString().Substring(0, 4)).Equals("Sure") || (message.ToString().Substring(0, 8)).Equals("Bardiche"))) return;
            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasCharPrefix('!', ref argPos)))
            {
                if ((message.ToString().Length < 4) || !((message.ToString().Substring(0, 4)).Equals("Sure") || ((message.ToString().Length > 7) && (message.ToString().Substring(0, 8)).Equals("Bardiche"))) || ((message.ToString().Length > 5) && (message.ToString().Substring(0, 6)).Equals("Surely"))) return;
            }
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