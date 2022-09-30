using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Database;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rendy.Common;
using Rendy.Services;
using Rendy.Utilities;

namespace Rendy.Modules
{
    public class UtilitiesModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly Servers _servers;
        private readonly IConfiguration _config;
        private readonly Images _images;

        public UtilitiesModule(Servers servers, IConfiguration config, Images images, DiscordSocketClient client)
        {
            _client = client;
            _servers = servers;
            _config = config;
            _images = images;
        }

        [Command("ping", RunMode = RunMode.Async)]
        public async Task PingAsync()
        {
            var msg = await Context.Channel.SendMessageAsync("Calculating..");
            var msgTime = Context.Message.Timestamp.Millisecond;
            var newMsgTime = msg.Timestamp.Millisecond;
            string time = Math.Abs((newMsgTime - msgTime)).ToString();
            await msg.ModifyAsync(x => x.Content = $"Hello, <@!{Context.Message.Author.Id}>. I have ``{time}ms`` of delay.");
        }

        [Command("echo", RunMode = RunMode.Async)]
        [Alias("say")]
        public async Task EchoAsync([Remainder] string text)
        {
            await ReplyAsync(text);
        }

        [Command("shout", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.SendTTSMessages)]
        [RequireUserPermission(ChannelPermission.SendTTSMessages)]
        public async Task Shout([Remainder] string text)
        {
            await ReplyAsync(text, isTTS: true);
        }

        [Command("math", RunMode = RunMode.Async)]
        public async Task MathAsync([Remainder] string math)
        {
            var dt = new DataTable();
            var result = dt.Compute(math, null);

            await Context.Channel.SendSuccessAsync("Result", result.ToString());
        }

        [Command("support", RunMode = RunMode.Async)]
        [Alias("help")]
        public async Task Support()
        {
            string prefix = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.Prefix).FirstOrDefault() ?? _config["prefix"];
            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Help Command")
                        .WithIconUrl(ConstModule.GetIconUrl("info"));
                })
                .WithDescription("I see you are in need of help on how to use me. All my documentation is on my GitHub. If that's not enough, feel free to join my support discord server (recommended) or send a support form. All links below.")
                .AddField("**Server prefix**", prefix, true)
                .AddField("**Docs (GitHub)**", "[Click Here](https://github.com/TiagoRG/Rendy/blob/master/Documentation/Setup.md)", true)
                .AddField("**Support Server**", "[Click Here](https://discord.gg/3stDnz8)", true)
                .WithFooter(ConstModule.footer)
                .WithColor(Color.Blue)
                .Build();
            await Context.Message.Author.SendMessageAsync(embed: embed);
            await Context.Message.DeleteAsync();
            IMessage reply = await ReplyAsync("Check our dm's for support information!");
            await Task.Delay(2500);
            await reply.DeleteAsync();
        }

        [Command("invite", RunMode = RunMode.Async)]
        public async Task Invite()
        {
            await Context.Message.DeleteAsync();
            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Blue)
                .WithTitle("Invite Rendy")
                .WithDescription("Add Rendy to your server with the link below!")
                .WithFooter(ConstModule.footer)
                .AddField("Invite Link", "[Click Here](https://discordapp.com/api/oauth2/authorize?client_id=699707685360369695&permissions=2147483639&redirect_uri=https%3A%2F%2Fdiscordapp.com%2Fapi%2Foauth2%2Fauthorize%3Fclient_id%3D699707685360369695%26permissions%3D8%26scope%3Dbot&scope=bot)")
                .Build();
            await Context.Channel.SendMessageAsync(embed: embed);
        }

        [Command("userinfo", RunMode = RunMode.Async)]
        [Alias("user")]
        [RequireContext(ContextType.Guild)]
        public async Task UserInfo(IUser user = null)
        {
            user ??= Context.Message.Author;

            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Blue)
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithTitle(user.Username + "#" + user.Discriminator)
                .WithFooter(ConstModule.footer)
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

        [Command("serverinfo", RunMode = RunMode.Async)]
        [Alias("server")]
        [RequireContext(ContextType.Guild)]
        public async Task ServerInfo()
        {
            var roles = Context.Guild.Roles;
            IRole[] roleList = { };
            string roleListOut = null;

            foreach (IRole role in roles)
            {
                if (roleList.Count() == 0)
                {
                    roleList.Append(role);
                    continue;
                }
                if (role.Position > roleList[0].Position)
                {
                    roleList.Prepend(role);
                    continue;
                }
                else
                {
                    roleList.Append(role);
                }
            }

            for (int j = 0; j < roleList.Count(); j++)
            {
                roleListOut = roleList[j].Mention + " ";
            }

            var invites = await Context.Guild.GetInvitesAsync();
            string inviteList = null;

            int i = 1;
            foreach (IInvite invite in invites)
            {
                // if (invite) continue;
                if (i == 1)
                {
                    inviteList = invite.Url;
                    i++;
                    continue;
                }

                inviteList += " " + invite.Url;
                i++;
            }

            var embed = new EmbedBuilder()
                .WithAuthor(author => {
                    author
                        .WithName("Server Info")
                        .WithIconUrl(ConstModule.GetIconUrl("info"));
                })
                .WithColor(Color.Blue)
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithTitle(Context.Guild.Name)
                .WithFooter(ConstModule.footer)
                .AddField("Created At", $"{Context.Guild.CreatedAt.Day}/{Context.Guild.CreatedAt.Month}/{Context.Guild.CreatedAt.Year}", true)
                .AddField("Days of Life", (Context.Message.CreatedAt - Context.Guild.CreatedAt).TotalDays.ToString().Split(".")[0], true)
                .AddField("Member Count", Context.Guild.MemberCount, true)
                .AddField("Guild ID", Context.Guild.Id, true)
                .AddField("Guild Owner", Context.Guild.Owner, true)
                .AddField("Region", Context.Guild.VoiceRegionId, true)
                .AddField("Categories", Context.Guild.CategoryChannels.Count, true)
                .AddField("Text Channels", Context.Guild.TextChannels.Count, true)
                .AddField("Voice Channels", Context.Guild.VoiceChannels.Count, true)
                .AddField("Active Invites", inviteList ?? "*No invites found.*")
                .AddField("Role List", roleListOut ?? "*No roles found.*")
                .Build();
            await Context.Channel.SendMessageAsync(embed: embed);
            await Context.Message.DeleteAsync();
        }

        [Command("roleinfo", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task RoleInfo(IRole role)
        {
            var permissions = role.Permissions.ToList();
            string permString = null;

            foreach (GuildPermission perm in permissions)
            {
                permString += $"__{perm}__; ";
            }

            if (role.Permissions.Administrator == true)
            {
                permString = "**__Administrator__**";
            }

            var embed = new EmbedBuilder()
                .WithAuthor(author => {
                    author
                        .WithName("Role Info")
                        .WithIconUrl(ConstModule.GetIconUrl("info"));
                })
                .WithColor(role.Color)
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithTitle(role.Name)
                .WithFooter(ConstModule.footer)
                .AddField("Role ID", role.Id, true)
                .AddField("Color", role.Color, true)
                .AddField("Created At", $"{role.CreatedAt.Day}/{role.CreatedAt.Month}/{role.CreatedAt.Year}", true)
                .AddField("Bot Role", role.IsManaged, true)
                .AddField("Mentionable", role.IsMentionable, true)
                .AddField("Hoited", role.IsHoisted, true)
                .AddField("Permissions", permString ?? "*This role has no permissions.*")
                .Build();
            await Context.Channel.SendMessageAsync(embed: embed);
            await Context.Message.DeleteAsync();
        }

        [Command("rank", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Rank([Remainder]string identifier)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = CommandHandler.RankList.Where(x => x.ServerId == Context.Guild.Id);

            IRole role;

            if(ulong.TryParse(identifier, out ulong roleId))
            {
                var roleById = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
                if(roleById == null)
                {
                    await ReplyAsync("That role does not exist!");
                    return;
                }

                role = roleById;
            }
            else
            {
                var roleByName = Context.Guild.Roles.FirstOrDefault(x => x.Name == identifier);
                if(roleByName == null)
                {
                    await ReplyAsync("That role does not exist!");
                    return;
                }

                role = roleByName;
            }

            if(ranks.Any(x => x.RoleId != role.Id))
            {
                await ReplyAsync("That rank does not exist!");
                return;
            }

            if(!(Context.User as SocketGuildUser).Roles.Any(x => x.Id == role.Id))
            {
                await (Context.User as SocketGuildUser).AddRoleAsync(role);
                await ReplyAsync($"Succesfully added the rank {role.Mention} to you.");
                return;
            }
            await (Context.User as SocketGuildUser).RemoveRoleAsync(role);
            await ReplyAsync($"Succesfully removed the rank {role.Mention} from you.");
        }
    }
}