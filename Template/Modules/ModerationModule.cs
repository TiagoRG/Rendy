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
        private readonly MuteWhitelists _muteWhitelists;
        private readonly MuteWhitelistsHelper _muteWhitelistsHelper;
        private readonly Servers _servers;
        private readonly IConfiguration _config;
        private readonly Mutes _mutes;
        private readonly RestoreRoles _restoreRoles;

        public ModerationModule(ILogger<ModerationModule> logger, MuteWhitelists muteWhitelists, MuteWhitelistsHelper muteWhitelistsHelper, Servers servers, IConfiguration config, Mutes mutes, RestoreRoles restoreRoles)
        {
            _logger = logger;
            _muteWhitelists = muteWhitelists;
            _muteWhitelistsHelper = muteWhitelistsHelper;
            _servers = servers;
            _config = config;
            _mutes = mutes;
            _restoreRoles = restoreRoles;
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

        [Command("ban", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Ban(IUser user, [Remainder] string reason = null)
        {
            await Context.Message.DeleteAsync();
            Embed embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Red)
                .WithTitle(user.Username + "#" + user.Discriminator)
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithDescription($"This user got banned from {Context.Guild.Name}")
                .WithFooter(ConstModule.footer)
                .AddField("Reason", reason)
                .AddField("Moderator", Context.Message.Author, true)
                .AddField("Banned At", $"{Context.Message.Timestamp.Day}/{Context.Message.Timestamp.Month}/{Context.Message.Timestamp.Year}", true)
                .Build();
            Embed privEmbed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Red)
                .WithTitle(user.Username + "#" + user.Discriminator)
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription($"You got banned from {Context.Guild.Name}")
                .WithFooter(ConstModule.footer)
                .AddField("Reason", reason)
                .AddField("Moderator", Context.Message.Author, true)
                .AddField("Banned At", $"{Context.Message.Timestamp.Day}/{Context.Message.Timestamp.Month}/{Context.Message.Timestamp.Year}", true)
                .Build();
            await user.SendMessageAsync(embed: privEmbed);
            await Context.Channel.SendMessageAsync(embed: embed);
            await Context.Guild.GetUser(user.Id).BanAsync(7, reason: reason);
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
        public async Task Mute(SocketGuildUser user, int minutes, [Remainder] string reason = null)
        {
            while (CommandHandler.AutoModTaskAvailable == false) { }
            CommandHandler.CommandTaskAvailable = false;

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

            var whitelistId = await _muteWhitelistsHelper.GetMuteWhitelistAsync(Context.Guild);

            foreach (var channel in Context.Guild.TextChannels)
            {
                if (whitelistId.Any(x => x.Id == channel.Id))
                    await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Allow));
                else if (!channel.GetPermissionOverwrite(role).HasValue || channel.GetPermissionOverwrite(role).Value.SendMessages == PermValue.Allow)
                    await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny));
            }
            foreach (var channel in Context.Guild.VoiceChannels)
            {
                if (!channel.GetPermissionOverwrite(role).HasValue || channel.GetPermissionOverwrite(role).Value.Speak == PermValue.Allow)
                    await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(speak: PermValue.Deny));
            }

            DateTime dateEnd = DateTime.Now + TimeSpan.FromMinutes(minutes);
            DateTime dateBegin = DateTime.Now;

            IEnumerable<IRole> restoreRoles = user.Roles;
            List<ulong> restoreRolesId = new List<ulong>();

            foreach (IRole removeRole in restoreRoles)
            {
                if (removeRole.Id == Context.Guild.Id || removeRole.IsManaged == true)
                    continue;
                restoreRolesId.Add(removeRole.Id);
                await user.RemoveRoleAsync(removeRole);
            }

            int counter = await _servers.GetMuteCounter(Context.Guild.Id);
            await _mutes.AddMuteAsync(Context.Guild.Id, counter, user.Id, Context.Message.Author.Id, role.Id, dateBegin, dateEnd, reason ?? "*No reason was given!*");
            int muteId = await _mutes.GetMuteIdAsync(Context.Guild.Id, user.Id);
            await _restoreRoles.AddRestoreRolesAsync(muteId, restoreRolesId);
            await _servers.AddMuteCounter(Context.Guild.Id);
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

            var channelId = await _servers.GetModLogsAsync(Context.Guild.Id);
            if (channelId != 0) await Context.Guild.GetTextChannel(channelId).SendMessageAsync(embed: embed);

            CommandHandler.CommandTaskAvailable = true;
            await Task.Delay(10000);
            await reply.DeleteAsync();
            
        }

        [Command("unmute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
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

            MuteData mute = await _mutes.GetMuteAsync(Context.Guild.Id, user.Id);

            List<ulong> restoreRolesId = await _restoreRoles.GetRestoreRolesIdAsync(mute.Id);

            if (restoreRolesId != null)
            {
                foreach (ulong restoreRoleId in restoreRolesId)
                {
                    try
                    {
                        await user.AddRoleAsync(Context.Guild.GetRole(restoreRoleId));
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

            var channelId = await _servers.GetModLogsAsync(Context.Guild.Id);
            if (channelId != 0) await Context.Guild.GetTextChannel(channelId).SendMessageAsync(embed: embed);

            await Task.Delay(10000);
            await reply.DeleteAsync();
        }
    }
}
