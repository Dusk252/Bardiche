using Bardiche.Classes;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Collections;

namespace Bardiche.Modules
{
    public class AdministrationModule : ModuleBase
    {

        [Command("delbot")]
        [Summary("Prunes bot messages.")]
        public async Task DelBot()
        {
            if (!Extensions.config_values.admin_ids.Contains(Context.User.Id))
                return;
            //var user = await Context.Guild.GetCurrentUserAsync().ConfigureAwait(false);
            IEnumerable messages = await Context.Channel.GetMessagesAsync().FlattenAsync();
            foreach (IMessage m in messages)
                if (Extensions.config_values.bot_ids.Contains(m.Author.Id))
                        await Context.Channel.DeleteMessageAsync(m).ConfigureAwait(false);
            Context.Message.DeleteAfter(3);
        }

        [Command("prune")]
        [Alias("clear", "clr")]
        [Summary("Prunes messages.")]
        public async Task Prune(int count)
        {
            if (count < 1 || !Extensions.config_values.admin_ids.Contains(Context.User.Id))
                return;
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            int limit = (count < 100) ? count : 100;
            IEnumerable messages = await Context.Channel.GetMessagesAsync(limit).FlattenAsync();
            foreach (IMessage m in messages)
                await Context.Channel.DeleteMessageAsync(m).ConfigureAwait(false);
        }

        [Command("del")]
        [Summary("Prunes user messages.")]
        public async Task Del(int count)
        {
            IUser user = Context.User;
            if (count < 1)
                return;

            if (user.Id == Context.User.Id)
                count += 1;

            IEnumerable messages = await Context.Channel.GetMessagesAsync().FlattenAsync();
            foreach (IMessage m in messages)
            {
                if (count == 0)
                    break;
                if (m.Author == user)
                {
                    await Context.Channel.DeleteMessageAsync(m).ConfigureAwait(false);
                    count--;
                }
            }

            Context.Message.DeleteAfter(3);
        }

    }
}