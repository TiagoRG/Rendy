using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Rendy.Modules
{
    public class UtilitiesModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<UtilitiesModule> _logger;

        public UtilitiesModule(ILogger<UtilitiesModule> logger)
        {
            _logger = logger;
        }

        [Command("ping")]
        public async Task PingAsync()
        {
            var msg = await Context.Channel.SendMessageAsync("Calculating..");
            var msgTime = Context.Message.Timestamp.Millisecond;
            var newMsgTime = msg.Timestamp.Millisecond;
            string time = (newMsgTime - msgTime).ToString();
            await msg.ModifyAsync(x => x.Content = $"Hello, <@!{Context.Message.Author.Id}>. I have ``{time}ms`` of delay.");
        }

        [Command("echo")]
        public async Task EchoAsync([Remainder] string text)
        {
            await ReplyAsync(text);
        }

        [Command("math")]
        public async Task MathAsync([Remainder] string math)
        {
            var dt = new DataTable();
            var result = dt.Compute(math, null);

            await ReplyAsync($"Result: {result}");
        }

        [Command("support")]
        [Alias("help")]
        public async Task Support()
        {
            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithDescription("I see you are in need of help on how to use me. All my documentation is on my GitHub. If that's not enough, feel free to join my support discord server (recommended) or send a support form. All links below.")
                .AddField("**Docs (GitHub)**", "[Click Here](https://github.com/TiagoRG/Rendy/blob/master/Documentation/Setup.md)", true)
                .AddField("**Support Server**", "Fastest Way\n[Click Here](https://discord.gg/3stDnz8)", true)
                .AddField("**Support Form**", "[Click Here](https://forms.office.com/Pages/ResponsePage.aspx?id=DQSIkWdsW0yxEjajBLZtrQAAAAAAAAAAAAO__SHXmw5UQUZGVk1TWjA2Tk02NUgzSVRDVUtXV1NDVi4u)", true)
                .WithThumbnailUrl(ConstModule.logoUrl)
                .WithFooter(ConstModule.embedFooter)
                .WithColor(Color.Blue)
                .Build();
            await Context.Message.Author.SendMessageAsync(embed: embed);
            await Context.Message.DeleteAsync();
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
                .WithFooter(ConstModule.embedFooter)
                .AddField("Invite Link", "[Click Here](https://discordapp.com/api/oauth2/authorize?client_id=699707685360369695&permissions=2147483639&redirect_uri=https%3A%2F%2Fdiscordapp.com%2Fapi%2Foauth2%2Fauthorize%3Fclient_id%3D699707685360369695%26permissions%3D8%26scope%3Dbot&scope=bot)")
                .Build();
            await Context.Channel.SendMessageAsync(embed: embed);
        }

        [Command("userinfo")]
        [Alias("user")]
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
                    .WithFooter(ConstModule.embedFooter)
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
                    .WithFooter(ConstModule.embedFooter)
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
        [Alias("server")]
        [RequireContext(ContextType.Guild)]
        public async Task ServerInfo()
        { 
            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Blue)
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithTitle(Context.Guild.Name)
                .WithFooter(ConstModule.embedFooter)
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