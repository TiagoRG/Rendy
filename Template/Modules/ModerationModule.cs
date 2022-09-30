using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Rendy.Common;
using Database;
using Rendy.Utilities;
using Rendy.Services;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using static Database.Mutes;

namespace Rendy.Modules
{
    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ModerationModule> _logger;
        private readonly Servers _servers;
        private readonly IConfiguration _config;

        public ModerationModule(ILogger<ModerationModule> logger, Servers servers, IConfiguration config)
        {
            _logger = logger;
            _servers = servers;
            _config = config;
        }

        [Command("purge", RunMode = RunMode.Async)]
        [Alias("clean")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task Purge(int limit = 100)
        {
            var messages = await Context.Channel.GetMessagesAsync(limit + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var reply = await Context.Channel.SendSuccessAsync("Channel Cleaned", $"{messages.Count() - 1} messages got deleted successfully. Enjoy your clean chat!");
            await Task.Delay(5000);
            await reply.DeleteAsync();
        }

        [Command("prune", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task Prune(int daysoff = 14)
        {
            await Context.Message.DeleteAsync();
            var totalKicks = await Context.Guild.PruneUsersAsync(daysoff);
            var reply = await Context.Channel.SendSuccessAsync("Prune Module", $"I kicked {totalKicks} members for being offline for more than {daysoff} days.");
            await Task.Delay(5000);
            await reply.DeleteAsync();
        }

        [Command("kick", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Kick(IUser user, [Remainder] string reason = null)
        {
            await Context.Message.DeleteAsync();
            Embed embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Red)
                .WithTitle(user.Username + "#" + user.Discriminator)
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithDescription($"This user got kicked from {Context.Guild.Name} so you are able to rejoin. Do you think you are not going to break rules again? Rejoin with the link in field \"Rejoin Invite\"")
                .WithFooter(ConstModule.footer)
                .AddField("Reason", reason)
                .AddField("Moderator", Context.Message.Author, true)
                .AddField("Kicked At", $"{Context.Message.Timestamp.Day}/{Context.Message.Timestamp.Month}/{Context.Message.Timestamp.Year}", true)
                .AddField("Rejoin Invite", "[Click Here](https://discord.gg/3stDnz8)")
                .Build();
            Embed privEmbed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Red)
                .WithTitle(user.Username + "#" + user.Discriminator)
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription($"You got kicked from {Context.Guild.Name}")
                .WithFooter(ConstModule.footer)
                .AddField("Reason", reason)
                .AddField("Moderator", Context.Message.Author, true)
                .AddField("Kicked At", $"{Context.Message.Timestamp.Day}/{Context.Message.Timestamp.Month}/{Context.Message.Timestamp.Year}", true)
                .Build();
            await user.SendMessageAsync(embed: privEmbed);
            await Context.Channel.SendMessageAsync(embed: embed);
            await Context.Guild.GetUser(user.Id).KickAsync(reason);
        }

        [Command("softban", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task SoftBan(IUser user, int days = 14, [Remainder] string reason = null)
        {
            await Context.Message.DeleteAsync();
            Embed embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Red)
                .WithTitle(user.Username + "#" + user.Discriminator)
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithDescription($"This user got softbanned from {Context.Guild.Name} to delete their messages.")
                .WithFooter(ConstModule.footer)
                .AddField("Reason", reason)
                .AddField("Moderator", Context.Message.Author, true)
                .AddField("Days Pruned", days, true)
                .AddField("SoftBanned At", $"{Context.Message.Timestamp.Day}/{Context.Message.Timestamp.Month}/{Context.Message.Timestamp.Year}", true)
                .Build();
            Embed privEmbed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Red)
                .WithTitle(user.Username + "#" + user.Discriminator)
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription($"You got softbanned from {Context.Guild.Name} to delete your messages so you are able to rejoin the server. Do you think you are not going to break rules again? Rejoin with the link in field \"Rejoin Invite\"")
                .WithFooter(ConstModule.footer)
                .AddField("Reason", reason)
                .AddField("Moderator", Context.Message.Author, true)
                .AddField("Banned At", $"{Context.Message.Timestamp.Day}/{Context.Message.Timestamp.Month}/{Context.Message.Timestamp.Year}", true)
                .AddField("Days Pruned", days, true)
                .AddField("Rejoin Invite", "[Click Here](https://discord.gg/3stDnz8)")
                .Build();
            await user.SendMessageAsync(embed: privEmbed);
            await Context.Channel.SendMessageAsync(embed: embed);
            await Context.Guild.GetUser(user.Id).BanAsync(days, reason: reason);
            await Context.Guild.RemoveBanAsync(user);
        }

        [Command("mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task Mute(SocketGuildUser user, int minutes, [Remainder] string reason = null)
        {
            if (CommandHandler.ServerList.Any(x => x.Id == Context.Guild.Id) == false)
                CommandHandler.ServerList.Add(new Server { Id = Context.Guild.Id });
            if (user.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid user", "That user is higher than me in the hierarchy");
                return;
            }
            if (user.Hierarchy > (Context.Message.Author as SocketGuildUser).Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid user", "That user is higher than you in the hierarchy");
                return;
            }

            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
            if (role == null)
                role = await Context.Guild.CreateRoleAsync("Muted", new GuildPermissions(sendMessages: false, speak: false), null, false, null);

            if (role.Permissions.SendMessages == true || role.Permissions.Speak == true)
                await role.ModifyAsync(x => x.Permissions = new GuildPermissions(sendMessages: false, speak: false));

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid permissions", "The **Muted** role has a higher position than the bot.");
                return;
            }

            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendErrorAsync("Invalid user", "That user is already muted.");
                return;
            }

            await role.ModifyAsync(x => x.Position = Context.Guild.CurrentUser.Hierarchy);

            var whitelistId = CommandHandler.MuteWhitelistList.Where(x => x.ServerId == Context.Guild.Id);
            foreach (var channelCheck in Context.Guild.TextChannels)
            {
                if (whitelistId.Any(x => x.ChannelId == channelCheck.Id))
                    await channelCheck.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Allow));
                else if (!channelCheck.GetPermissionOverwrite(role).HasValue && channelCheck.GetPermissionOverwrite(role).Value.SendMessages == PermValue.Allow)
                    await channelCheck.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny));
            }
            foreach (var channelCheck in Context.Guild.VoiceChannels)
            {
                if (!channelCheck.GetPermissionOverwrite(role).HasValue && channelCheck.GetPermissionOverwrite(role).Value.Speak == PermValue.Allow)
                    await channelCheck.AddPermissionOverwriteAsync(role, new OverwritePermissions(speak: PermValue.Deny));
            }

            DateTime dateEnd = DateTime.Now + TimeSpan.FromMinutes(minutes);
            DateTime dateBegin = DateTime.Now;

            int counter = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.MuteId).FirstOrDefault();
            counter++;
            CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).ToList().ForEach(x => x.MuteId = counter);

            IEnumerable<IRole> restoreRoles = user.Roles;

            foreach (IRole removeRole in restoreRoles)
            {
                if (removeRole.Id == Context.Guild.Id || removeRole.IsManaged == true)
                    continue;
                CommandHandler.RestoreRoleList.Add(new RestoreRole { MuteId = counter, RoleId = removeRole.Id });
                await user.RemoveRoleAsync(removeRole);
            }

            CommandHandler.MuteList.Add(new Mute { ServerId = Context.Guild.Id, MuteId = counter, UserId = user.Id, ModId = Context.Guild.CurrentUser.Id, RoleId = role.Id, Begin = dateBegin, End = dateEnd, Reason = reason ?? "*No reason was given!*" });
            await user.AddRoleAsync(role);

            Embed embed = new EmbedBuilder()
                .WithColor(new Color(82, 196, 26))
                .WithAuthor(author =>
                {
                    author
                        .WithName($"Mute #{counter}")
                        .WithIconUrl("https://icons.veryicon.com/png/o/miscellaneous/cloud-call-center/success-24.png");
                })
                .WithTitle("**User Muted**")
                .WithDescription($"Mute occurred at {dateBegin}. Muted until {dateEnd}.")
                .AddField("Muted Username", user.Mention, true)
                .AddField("Moderator", Context.Message.Author.Mention, true)
                .AddField("Duration", $"{minutes} minutes", true)
                .AddField("Reason", reason ?? "*No reason was given!*")
                .WithFooter(ConstModule.footer)
                .Build();

            var reply = await ReplyAsync(embed: embed);

            var channelId = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.ModLogs).FirstOrDefault();
            if (channelId != 0) await Context.Guild.GetTextChannel(channelId).SendMessageAsync(embed: embed);

            await Task.Delay(10000);
            await reply.DeleteAsync();
            
        }

        [Command("unmute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task Unmute(SocketGuildUser user)
        {
            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
            if (role == null)
            {
                await Context.Channel.SendErrorAsync("Not muted", "That user is not currently muted.");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid permissions", "The **Muted** role has a higher position than the bot.");
                return;
            }

            if (!user.Roles.Contains(role))
            {
                await Context.Channel.SendErrorAsync("Not muted", "That user is not currently muted.");
                return;
            }

            Mute mute = CommandHandler.MuteList
                .Where(x => x.ServerId == Context.Guild.Id)
                .Where(x => x.UserId == user.Id)
                .FirstOrDefault();

            List<RestoreRole> restoreRoles = CommandHandler.RestoreRoleList.Where(x => x.MuteId == mute.MuteId).ToList();

            if (restoreRoles != null)
            {
                foreach (RestoreRole restoreRole in restoreRoles)
                {
                    try
                    {
                        await user.AddRoleAsync(Context.Guild.GetRole(restoreRole.RoleId));
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            await user.RemoveRoleAsync(Context.Guild.GetRole(mute.RoleId));

            Embed embed = new EmbedBuilder()
                .WithColor(new Color(245, 34, 45))
                .WithAuthor(author =>
                {
                    author
                        .WithName($"Mute #{mute.MuteId}")
                        .WithIconUrl("https://icons.veryicon.com/png/o/miscellaneous/cloud-call-center/error-43.png");
                })
                .WithTitle("**User Unmuted**")
                .WithDescription($"Mute occurred at {DateTime.Now}")
                .AddField("Unmuted Username", user.Mention, true)
                .AddField("Moderator", Context.Message.Author.Mention, true)
                .AddField("Duration", $"{mute.End.Minute - mute.Begin.Minute} minutes", true)
                .AddField("Reason", mute.Reason ?? "*No reason was given!*")
                .WithFooter(ConstModule.footer)
                .Build();

            var reply = await ReplyAsync(embed: embed);

            var channelId = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.ModLogs).FirstOrDefault();
            if (channelId != 0) await Context.Guild.GetTextChannel(channelId).SendMessageAsync(embed: embed);

            await Task.Delay(10000);
            await reply.DeleteAsync();
        }

        [Command("ban", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Ban(SocketGuildUser user, int minutes = 0, [Remainder] string reason = null)
        {
            if (CommandHandler.ServerList.Any(x => x.Id == Context.Guild.Id) == false)
                CommandHandler.ServerList.Add(new Server { Id = Context.Guild.Id });
            if (user.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid user", "That user is higher than me in the hierarchy");
                return;
            }
            if (user.Hierarchy > (Context.Message.Author as SocketGuildUser).Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid user", "That user is higher than you in the hierarchy");
                return;
            }

            DateTime dateEnd = DateTime.Now + TimeSpan.FromMinutes(minutes);
            DateTime dateBegin = DateTime.Now;

            int counter = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.MuteId).FirstOrDefault();
            counter++;
            CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).ToList().ForEach(x => x.MuteId = counter);

            if (dateBegin != dateEnd)
            {
                Embed embed = new EmbedBuilder()
                    .WithColor(new Color(82, 196, 26))
                    .WithAuthor(author =>
                    {
                        author
                            .WithName($"Ban #{counter}")
                            .WithIconUrl("https://icons.veryicon.com/png/o/miscellaneous/cloud-call-center/success-24.png");
                    })
                    .WithTitle("**User Banned**")
                    .WithDescription($"Ban occurred at {dateBegin}. Banned until {dateEnd}.")
                    .AddField("Banned Username", user.Mention, true)
                    .AddField("Moderator", Context.Message.Author.Mention, true)
                    .AddField("Duration", $"{minutes} minutes", true)
                    .AddField("Reason", reason ?? "*No reason was given!*")
                    .WithFooter(ConstModule.footer)
                    .Build();

                var reply = await ReplyAsync(embed: embed);
                await user.SendMessageAsync(embed: embed);
                var channelId = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.ModLogs).FirstOrDefault();
                if (channelId != 0) await Context.Guild.GetTextChannel(channelId).SendMessageAsync(embed: embed);

                CommandHandler.BanList.Add(new Ban { BanId = counter, ServerId = Context.Guild.Id, UserId = user.Id, ModId = Context.Message.Author.Id, Reason = reason, Begin = dateBegin, End = dateEnd });
                await Context.Guild.AddBanAsync(user, 7, reason);

                await Task.Delay(10000);
                await reply.DeleteAsync();
            }
            else
            {
                Embed embed = new EmbedBuilder()
                    .WithColor(new Color(82, 196, 26))
                    .WithAuthor(author =>
                    {
                        author
                            .WithName($"Ban #{counter}")
                            .WithIconUrl("https://icons.veryicon.com/png/o/miscellaneous/cloud-call-center/success-24.png");
                    })
                    .WithTitle("**User Banned**")
                    .WithDescription($"Ban occurred at {dateBegin}. Banned permanently.")
                    .AddField("Banned Username", user.Mention, true)
                    .AddField("Moderator", Context.Message.Author.Mention, true)
                    .AddField("Duration", "**--------**", true)
                    .AddField("Reason", reason ?? "*No reason was given!*")
                    .WithFooter(ConstModule.footer)
                    .Build();

                var reply = await ReplyAsync(embed: embed);
                await user.SendMessageAsync(embed: embed);
                var channelId = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.ModLogs).FirstOrDefault();
                if (channelId != 0) await Context.Guild.GetTextChannel(channelId).SendMessageAsync(embed: embed);

                CommandHandler.BanList.Add(new Ban { BanId = counter, ServerId = Context.Guild.Id, UserId = user.Id, ModId = Context.Message.Author.Id, Reason = reason, Begin = dateBegin, End = dateEnd + TimeSpan.FromDays(3650) });
                await Context.Guild.AddBanAsync(user, 7, reason);

                await Task.Delay(10000);
                await reply.DeleteAsync();
            }
        }
    }
}
