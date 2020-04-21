using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rendy
{
    public class TrollingModule : ModuleBase<SocketCommandContext>
    {
        [Command("spam")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Spam(string user, string stringTimes)
        {
            string[] messages = { "Stop spamming", "That's what you get for spamming", "You suck", "Look who is spamming know", "Don't you agree that this is funny" };
            int times = Convert.ToInt32(stringTimes);
            Random random = new Random();
            if (times > 25)
            {
                for (int x = 0; x < 25; x++)
                {
                    await Context.Channel.SendMessageAsync($"{messages[random.Next(5)]}, {user}");
                }
            }
            else
            {
                for (int x = 0; x < times; x++)
                {
                    await Context.Channel.SendMessageAsync($"{messages[random.Next(5)]}, {user}");
                }
            }
        }
    }
}
