using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Database;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Rendy.Common;
using Rendy.Modules;
using Rendy.Utilities;
using Victoria;

namespace Rendy.Modules
{
    public class OwnerModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;
        private readonly Mutes _mutes;

        public OwnerModule(DiscordSocketClient client, IConfiguration config, Mutes mutes)
        {
            _client = client;
            _config = config;
            _mutes = mutes;
        }

        [Command("tell", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task Tell(IUser user, [Remainder] string msg)
        {
            Embed embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.DarkerGrey)
                .WithDescription($"You got a message from Rendy Owner!")
                .WithTitle("TiagoRG")
                .WithFooter(ConstModule.footer)
                .AddField("**Message Content**", msg)
                .AddField("**Sent At**", Context.Message.Timestamp)
                .WithThumbnailUrl(user.GetAvatarUrl())
                .Build();
            await Context.Message.DeleteAsync();
            await user.SendMessageAsync(embed: embed);
            await Context.Channel.SendMessageAsync("Message sent!");
            await Context.Channel.SendMessageAsync(embed: embed);
        }

        [Command("leaveserver", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task LeaveServer()
        {
            await Context.Channel.SendMessageAsync($"Bye {Context.Guild.Name}, see you next time!");
            await Context.Message.Author.SendMessageAsync("Just left the server successfully.");
            await Context.Guild.LeaveAsync();
        }

        [Command("shutdown", RunMode = RunMode.Sync)]
        [RequireOwner]
        public async Task Shutdown(int countdown = 0)
        {
            int numCountdown = Convert.ToInt32(countdown);
            await ReplyAsync($"Bot is shutting down in {numCountdown} seconds.");
            await Task.Delay(numCountdown * 1000);
            await ReplyAsync("Shutting down...");
            await _client.StopAsync();
        }

        [Command("test")]
        [RequireGuild(new ulong[] { 452245903219359745 })]
        public async Task TestCommand()
        {
            await _mutes.RemoveMuteAsync(Context.Guild.Id, Context.Message.Author.Id);
            // await _mutes.AddMuteAsync(Context.Guild.Id, Context.Message.Author.Id, Context.Message.Author.Id, 917832356772085831, DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(10), null, 452403573003649035, 493432570202161153);
            await ReplyAsync("done");
        }
    }
}
