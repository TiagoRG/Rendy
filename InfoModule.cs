using Discord;
using Discord.Commands;
using Discord.API;
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
            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithDescription("Here is the list of all Rendy commands. Prefix for all commands is ``/``")
                .AddField("**Info**", "> help -> Shows this help message\n> ping -> Shows the time bot takes to reply\n> support -> Sends information about how do you get Rendy support.")
                .WithThumbnailUrl(Const.logoUrl)
                .WithFooter($"Rendy{Const.copyright} by TiagoRG#8003")
                .WithColor(Color.Blue)
                .Build();
            await Context.Message.Author.SendMessageAsync(embed: embed);
            var reply = await Context.Channel.SendMessageAsync("Check your dm's for help!", true);
            await Context.Message.DeleteAsync();
            await Task.Delay(2500);
            await reply.DeleteAsync();
        }

        [Command("support")]
        /*[RequireContext(ContextType.Guild)]*/
        public async Task Support()
        {
            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithDescription("Join our discord server or send support form to get support about the usage of Rendy.")
                .AddField("**Support Server**", "Fastest Way\n[Click Here](https://discord.gg/3stDnz8)")
                .AddField("**Support Form**", "[Click Here](https://forms.office.com/Pages/ResponsePage.aspx?id=DQSIkWdsW0yxEjajBLZtrQAAAAAAAAAAAAO__SHXmw5UQUZGVk1TWjA2Tk02NUgzSVRDVUtXV1NDVi4u)")
                .WithThumbnailUrl(Const.logoUrl)
                .WithFooter($"Rendy{Const.copyright} by TiagoRG#8003")
                .WithColor(Color.Blue)
                .Build();
            await Context.Message.Author.SendMessageAsync(embed: embed);
            var reply = await Context.Channel.SendMessageAsync("Check your dm's for support info!", true);
            await Context.Message.DeleteAsync();
            await Task.Delay(2500);
            await reply.DeleteAsync();
        }
    }
}
