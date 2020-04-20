using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rendy
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping", RunMode = RunMode.Async)]
        [Summary("Ping the bot")]
        [RequireContext(ContextType.Guild)]
        public async Task Ping()
        {
            var msg = await Context.Channel.SendMessageAsync("Calculating..");
            var msgTime = Context.Message.Timestamp;
            var newMsgTime = msg.Timestamp;
            string[] time = (newMsgTime - msgTime).ToString().Split('.');
            string time2 = time[1].Substring(0, 3);
            await msg.ModifyAsync(x => x.Content = $"Hello, <@!{Context.Message.Author.Id}>. I have ``{time2}ms`` of delay.");
        }

        [Command("help", RunMode = RunMode.Async)]
        [Summary("Get the help command")]
        [RequireContext(ContextType.Guild)]
        public async Task Help()
        {
            await Context.Message.DeleteAsync();
            await Context.Message.Author.SendMessageAsync("**List for all my commands:** *(My prefix is ``/``)*\n ```json\n\"help\": \"Used to get this help message.\"\n\"ping\": \"Used to see what delay I have to react to users.\"```");
        }

        [Command("support", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task Support()
        {
            await Context.Message.DeleteAsync();
            await Context.Message.Author.SendMessageAsync("Join Rendy support server using https://discord.gg/3stDnz8");
        }
    }
}
