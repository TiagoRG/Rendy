using Discord;
using Discord.Commands;
using Discord.API;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Rendy
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping", RunMode = RunMode.Async)]
        [Summary("Ping the bot")]
        public async Task Ping()
        {
            var msg = await Context.Channel.SendMessageAsync("Calculating..");
            var msgTime = Context.Message.Timestamp.Millisecond;
            var newMsgTime = msg.Timestamp.Millisecond;
            string time = (newMsgTime - msgTime).ToString();
            await msg.ModifyAsync(x => x.Content = $"Hello, <@!{Context.Message.Author.Id}>. I have ``{time}ms`` of delay.");
        }

        [Command("help", RunMode = RunMode.Async)]
        [Summary("Get the help command")]
        public async Task Help()
        {
            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithDescription("Here is the list of all Rendy commands. Prefix for all commands is ``/``\n> <> represents required parameter\n> [] represents optional parameter\n> {} represents optional parameter but you have to incluse it to use later parameters")
                .AddField("**Info**", "> help -> Shows this help message\n> ping -> Shows the time bot takes to reply\n> support -> Sends information about how do you get Rendy support.")
                .AddField("**Utilities**", "> suggest -> sends a suggestion for server owner | /suggest <suggestion>")
                .AddField("**Moderation**", "> prune -> prunes all members that have been offline for long time | /prune [min offline days (default = 14)]\n> kick -> kicks an user | /kick <user> [reason]\n> ban -> bans an user | /ban <user> [reason]\n> softban -> bans an unbans an user to delete their messages (last 7 days messages) | /softban <user> <days to purge> [reason]")
                .AddField("**Trolling**", "> spam -> spams an user with mentions | /spam <@user> <amount of mentions>")
                .WithThumbnailUrl(Const.logoUrl)
                .WithFooter(Const.embedFooter)
                .WithColor(Color.Blue)
                .Build();
            await Context.Message.Author.SendMessageAsync(embed: embed);
            var reply = await Context.Channel.SendMessageAsync("Check your dm's for help!");
            await Context.Message.DeleteAsync();
            await Task.Delay(2500);
            await reply.DeleteAsync();
        }

        [Command("support")]
        public async Task Support()
        {
            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithDescription("Join our discord server or send support form to get support about the usage of Rendy.")
                .AddField("**Support Server**", "Fastest Way\n[Click Here](https://discord.gg/3stDnz8)", true)
                .AddField("**Support Form**", "[Click Here](https://forms.office.com/Pages/ResponsePage.aspx?id=DQSIkWdsW0yxEjajBLZtrQAAAAAAAAAAAAO__SHXmw5UQUZGVk1TWjA2Tk02NUgzSVRDVUtXV1NDVi4u)", true)
                .WithThumbnailUrl(Const.logoUrl)
                .WithFooter(Const.embedFooter)
                .WithColor(Color.Blue)
                .Build();
            await Context.Message.Author.SendMessageAsync(embed: embed);
            var reply = await Context.Channel.SendMessageAsync("Check your dm's for support info!");
            await Context.Message.DeleteAsync();
            await Task.Delay(2500);
            await reply.DeleteAsync();
        }

        [Command("invite")]
        public async Task Invite()
        {
            await Context.Message.DeleteAsync();
            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Blue)
                .WithTitle("Invite Rendy")
                .WithDescription("Add Rendy to your server with the link below!")
                .WithFooter(Const.embedFooter)
                .AddField("Invite Link", "[Click Here](https://discordapp.com/api/oauth2/authorize?client_id=699707685360369695&permissions=2147483639&redirect_uri=https%3A%2F%2Fdiscordapp.com%2Fapi%2Foauth2%2Fauthorize%3Fclient_id%3D699707685360369695%26permissions%3D8%26scope%3Dbot&scope=bot)")
                .Build();
            await Context.Channel.SendMessageAsync(embed: embed);
        }

        [Command("userinfo")]
        [RequireContext(ContextType.Guild)]
        public async Task UserInfo(IUser user = null)
        {
            if(user == null)
            {
                var embed = new EmbedBuilder()
                    .WithAuthor("Rendy")
                    .WithColor(Color.Blue)
                    .WithThumbnailUrl(Context.Message.Author.GetAvatarUrl())
                    .WithTitle(Context.Message.Author.Username + "#" + Context.Message.Author.Discriminator)
                    .WithFooter(Const.embedFooter)
                    .AddField("Joined at", $"{Context.Guild.GetUser(Context.Message.Author.Id).JoinedAt.GetValueOrDefault().Day}/{Context.Guild.GetUser(Context.Message.Author.Id).JoinedAt.GetValueOrDefault().Month}/{Context.Guild.GetUser(Context.Message.Author.Id).JoinedAt.GetValueOrDefault().Year}", true)
                    .AddField("Days in Guild", ((Context.Message.Timestamp - Context.Guild.GetUser(Context.Message.Author.Id).JoinedAt).GetValueOrDefault().TotalDays).ToString().Split(".")[0], true)
                    .AddField("Created at", $"{Context.Guild.GetUser(Context.Message.Author.Id).CreatedAt.Day}/{Context.Guild.GetUser(Context.Message.Author.Id).CreatedAt.Month}/{Context.Guild.GetUser(Context.Message.Author.Id).CreatedAt.Year}", true)
                    .AddField("Is Bot", Context.Message.Author.IsBot, true)
                    .AddField("User ID", Context.Message.Author.Id, true)
                    .AddField("Currently Playing", Context.Message.Author.Activity, true)
                    .Build();
                await Context.Channel.SendMessageAsync(embed: embed);
                await Context.Message.DeleteAsync();
            }
            else
            {
                var embed = new EmbedBuilder()
                    .WithAuthor("Rendy")
                    .WithColor(Color.Blue)
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithTitle(user.Username + "#" + user.Discriminator)
                    .WithFooter(Const.embedFooter)
                    .AddField("Joined at", $"{Context.Guild.GetUser(user.Id).JoinedAt.GetValueOrDefault().Day}/{Context.Guild.GetUser(user.Id).JoinedAt.GetValueOrDefault().Month}/{Context.Guild.GetUser(user.Id).JoinedAt.GetValueOrDefault().Year}", true)
                    .AddField("Days in Guild", ((Context.Message.Timestamp - Context.Guild.GetUser(user.Id).JoinedAt).GetValueOrDefault().TotalDays).ToString().Split(".")[0], true)
                    .AddField("Created at", $"{Context.Guild.GetUser(user.Id).CreatedAt.Day}/{Context.Guild.GetUser(user.Id).CreatedAt.Month}/{Context.Guild.GetUser(user.Id).CreatedAt.Year}", true)
                    .AddField("Is Bot", Context.Guild.GetUser(user.Id).IsBot, true)
                    .AddField("User ID", Context.Guild.GetUser(user.Id).Id, true)
                    .AddField("Currently Playing", Context.Guild.GetUser(user.Id).Activity, true)
                    .Build();
                await Context.Channel.SendMessageAsync(embed: embed);
                await Context.Message.DeleteAsync();
            }
        }

        [Command("serverinfo")]
        [RequireContext(ContextType.Guild)]
        public async Task ServerInfo()
        { 
            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Blue)
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithTitle(Context.Guild.Name)
                .WithFooter(Const.embedFooter)
                .AddField("Created At", $"{Context.Guild.CreatedAt.Day}/{Context.Guild.CreatedAt.Month}/{Context.Guild.CreatedAt.Year}", true)
                .AddField("Days of Life", (Context.Message.CreatedAt - Context.Guild.CreatedAt).TotalDays.ToString().Split(".")[0], true)
                .AddField("Member Count", Context.Guild.MemberCount, true)
                .AddField("Guild ID", Context.Guild.Id, true)
                .AddField("Guild Owner", Context.Guild.Owner, true)
                .AddField("Region", Context.Guild.VoiceRegionId, true)
                .AddField("Categories", Context.Guild.CategoryChannels.Count, true)
                .AddField("Text Channels", Context.Guild.TextChannels.Count, true)
                .AddField("Voice Channels", Context.Guild.VoiceChannels.Count, true)
                .AddField("Active Invites", Context.Guild.GetInvitesAsync())
                .Build();
            await Context.Channel.SendMessageAsync(embed: embed);
            await Context.Message.DeleteAsync();
        }
    }
}
