using Database;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Rendy.Utilities;
using System.Linq;
using System.Drawing;
using Rendy.Common;

namespace Rendy.Modules
{
    public class ConfigModule : ModuleBase<SocketCommandContext>
    {
        private readonly Servers _servers;
        private readonly IConfiguration _config;
        private readonly RanksHelper _ranksHelper;
        private readonly AutoRolesHelper _autoRolesHelper;
        private readonly Ranks _ranks;
        private readonly AutoRoles _autoRoles;
        private readonly MuteWhitelists _muteWhitelists;
        private readonly MuteWhitelistsHelper _muteWhitelistsHelper;
        private readonly ModSettings _modSettings;

        public ConfigModule(Servers servers, IConfiguration config, RanksHelper ranksHelper, AutoRolesHelper autoRolesHelper, Ranks ranks, AutoRoles autoRoles, MuteWhitelists muteWhitelists, MuteWhitelistsHelper muteWhitelistsHelper, ModSettings modSettings)
        {
            _servers = servers;
            _config = config;
            _ranksHelper = ranksHelper;
            _autoRolesHelper = autoRolesHelper;
            _ranks = ranks;
            _autoRoles = autoRoles;
            _muteWhitelists = muteWhitelists;
            _muteWhitelistsHelper = muteWhitelistsHelper;
            _modSettings = modSettings;
        }

        [Command("ranks", RunMode = RunMode.Async)]
        public async Task Ranks()
        {
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);
            var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? _config["prefix"];
            if (ranks.Count == 0)
            {
                await ReplyAsync($"This server has no ranks yet, use {guildPrefix}addrank in order to set up a rank");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string rankList = "";

            foreach(var rank in ranks)
            {
                rankList += $"\n{rank.Mention} ({rank.Id})";
            }

            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithTitle("Guild Ranks")
                .WithDescription("This page lists every rank on the guild")
                .AddField("List", rankList)
                .AddField("How to get them", $"Use {guildPrefix}rank <rank> in order to get a rank mentioned previously.")
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithFooter(ConstModule.footer)
                .WithColor((Discord.Color)ColorTranslator.FromHtml("#747474"))
                .Build();
            await ReplyAsync(embed: embed);
        }

        [Command("addrank")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist. (Use the name of the role, don't mention it)");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("That role has a higher position than me.");
                return;
            }

            if (ranks.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already a rank.");
                return;
            }

            await _ranks.AddRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been added to the ranks.");
        }

        [Command("delrank")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist. (Use the name of the role, don't mention it)");
                return;
            }

            if (ranks.Any(x => x.Id != role.Id))
            {
                await ReplyAsync("That role is not a rank yet.");
                return;
            }

            await _ranks.RemoveRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been removed from the ranks.");
        }

        [Command("autoroles", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AutoRoles()
        {
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);
            if (autoRoles.Count == 0)
            {
                await ReplyAsync("This server does not yet have any autoroles!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string autoRoleList = "";
            foreach (var autoRole in autoRoles)
            {
                autoRoleList += $"{autoRole.Mention}";
            }

            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithTitle("Guild Auto Roles")
                .WithDescription("This page lists every auto role on the guild")
                .AddField("List", autoRoleList)
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithFooter(ConstModule.footer)
                .WithColor((Discord.Color)ColorTranslator.FromHtml("#747474"))
                .Build();
            await ReplyAsync(embed: embed);
        }

        [Command("addautorole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("That role has a higher position than the bot!");
                return;
            }

            if (autoRoles.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already an autorole!");
                return;
            }

            await _autoRoles.AddAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been added to the autoroles!");
        }

        [Command("delautorole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }

            if (autoRoles.Any(x => x.Id != role.Id))
            {
                await ReplyAsync("That role is not an autorole yet!");
                return;
            }

            await _autoRoles.RemoveAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been removed from the autoroles!");
        }

        [Command("prefix", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task Prefix(string prefix = null)
        {
            if (prefix == null)
            {
                var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? _config["prefix"];
                var embed1 = new EmbedBuilder()
                    .WithAuthor("Rendy")
                    .WithTitle("Guild Prefix")
                    .WithDescription("All information about your guild prefix is on this embed page.")
                    .AddField("Current prefix", $"**{guildPrefix}**")
                    .AddField("How to change it?", $"{guildPrefix}prefix <new prefix>")
                    .WithColor((Discord.Color)ColorTranslator.FromHtml("#747474"))
                    .WithFooter(ConstModule.footer)
                    .Build();
                await Context.Channel.SendMessageAsync(embed: embed1);
                return;
            }
            if (prefix == await _servers.GetGuildPrefix(Context.Guild.Id))
            {
                var embed1 = new EmbedBuilder()
                    .WithAuthor("Rendy")
                    .WithTitle("Guild Prefix")
                    .WithDescription("You are trying to change the guild prefix to the same it was before.")
                    .AddField("Current prefix", $"**{prefix}**")
                    .WithColor((Discord.Color)ColorTranslator.FromHtml("#747474"))
                    .WithFooter(ConstModule.footer)
                    .Build();
                await Context.Channel.SendMessageAsync(embed: embed1);
                return;
            }

            if (prefix.Length > 3)
            {
                await ReplyAsync("The length of new prefix is too long, please do not use more than 3 characters!");
                return;
            }

            var currentGuildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? _config["prefix"];
            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            var embed2 = new EmbedBuilder()
                    .WithAuthor("Rendy")
                    .WithTitle("Guild Prefix")
                    .WithDescription("My prefix for this server have been updated successfully!")
                    .AddField("Previous prefix", $"**{currentGuildPrefix}**")
                    .AddField("New prefix", $"**{prefix}**")
                    .WithColor((Discord.Color)ColorTranslator.FromHtml("#747474"))
                    .WithFooter(ConstModule.footer)
                    .Build();
            await Context.Channel.SendMessageAsync(embed: embed2);
        }

        [Command("welcome", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task Welcome(string option = null, string value = null)
        {
            if (option == null && value == null)
            {
                var fetchedChannelId = await _servers.GetWelcomeAsync(Context.Guild.Id);
                if (fetchedChannelId == 0)
                {
                    await ReplyAsync("There has not been set a welcome channel yet.");
                    return;
                }

                var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                if (fetchedChannel == null)
                {
                    await ReplyAsync("There has not been set a welcome channel yet.");
                    await _servers.ClearWelcomeAsync(Context.Guild.Id);
                    return;
                }

                var fetchedBackground = await _servers.GetBackgroundAsync(Context.Guild.Id);
                if (fetchedBackground != null)
                    await ReplyAsync($"The channel used for the welcome module is {fetchedChannel.Mention}.\nThe background is set to [this]({fetchedBackground}).");
                else
                    await ReplyAsync($"The channel used for the welcome module is {fetchedChannel.Mention}.");

                return;
            }

            if (option == "channel" && value != null)
            {
                if (!MentionUtils.TryParseChannel(value, out ulong parsedId))
                {
                    await ReplyAsync("Please pass a valid channel.");
                    return;
                }

                var parsedChannel = Context.Guild.GetTextChannel(parsedId);
                if (parsedChannel == null)
                {
                    await ReplyAsync("Please pass a valid channel.");
                    return;
                }

                await _servers.ModifyWelcomeAsync(Context.Guild.Id, parsedId);
                await ReplyAsync($"Successfully modified the welcome channel to {parsedChannel.Mention}.");
                return;
            }
            if (option == "background" && value != null)
            {
                if (value == "clear")
                {
                    await _servers.ClearBackgroundAsync(Context.Guild.Id);
                    await ReplyAsync($"Successfully removed the background for this server.");
                    return;
                }

                await _servers.ModifyBackgroundAsync(Context.Guild.Id, value);
                await ReplyAsync($"Successfully modified the background to {value}.");
                return;
            }
            if (option == "clear")
            {
                await _servers.ClearWelcomeAsync(Context.Guild.Id);
                await ReplyAsync("Successfully removed the welcome channel.");
                return;
            }

            await ReplyAsync("You did not use this command properly.");
        }

        [Command("mutewhitelist", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task MuteWhitelist(string option = null, string channel = null)
        {
            string prefix = await _servers.GetGuildPrefix(Context.Guild.Id);
            if (option == "add")
            {
                if (channel == null)
                {
                    await Context.Channel.SendErrorAsync("Mute Whitelist", "Please pass a channel to be added into the whitelist.");
                    return;
                }

                if (!MentionUtils.TryParseChannel(channel, out ulong parsedId))
                {
                    await Context.Channel.SendErrorAsync("Mute Whitelist", "Please pass a valid channel.");
                    return;
                }

                var parsedChannel = Context.Guild.GetTextChannel(parsedId);
                if (parsedChannel == null)
                {
                    await Context.Channel.SendErrorAsync("Mute Whitelist", "Please pass a valid channel.");
                    return;
                }

                var whitelist = await _muteWhitelistsHelper.GetMuteWhitelistAsync(Context.Guild);
                if (whitelist.Any(x => x.Id == parsedChannel.Id))
                {
                    await Context.Channel.SendErrorAsync("Mute Whitelist", "That channel is already in the whitelist.");
                    return;
                }

                await _muteWhitelists.AddMuteWhitelistAsync(Context.Guild.Id, parsedId);
                await Context.Channel.SendSuccessAsync("Mute Whitelist", $"Successfully added {parsedChannel.Mention} to the whitelist.");
                return;
            }
            if (option == "remove")
            {
                if (channel == null)
                {
                    await Context.Channel.SendErrorAsync("Mute Whitelist", "Please pass a channel to be removed from the whitelist.");
                    return;
                }

                if (!MentionUtils.TryParseChannel(channel, out ulong parsedId))
                {
                    await Context.Channel.SendErrorAsync("Mute Whitelist", "Please pass a valid channel.");
                    return;
                }

                var parsedChannel = Context.Guild.GetTextChannel(parsedId);
                if (parsedChannel == null)
                {
                    await Context.Channel.SendErrorAsync("Mute Whitelist", "Please pass a valid channel.");
                    return;
                }

                var whitelist = await _muteWhitelistsHelper.GetMuteWhitelistAsync(Context.Guild);
                if (!whitelist.Any(x => x.Id == parsedChannel.Id))
                {
                    await Context.Channel.SendErrorAsync("Mute Whitelist", "That channel is not in the whitelist.");
                    return;
                }

                await _muteWhitelists.RemoveMuteWhitelistAsync(Context.Guild.Id, parsedId);
                await Context.Channel.SendSuccessAsync("Mute Whitelist", $"Successfully removed {parsedChannel.Mention} from the whitelist.");
                return;
            }

            await Context.Channel.SendInfoAsync("Mute Whitelist", $"Enter a valid option for the subcommand.\n> {prefix}whitelist add <channel> - to add a new channel to the whitelist\n> {prefix}whitelist remove <channel> - to remove 1 channel from the whitelist");
        }

        [Command("logs", RunMode = RunMode.Async)]
        [Alias("log", "logging")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Logs(string option1 = null, string option2 = null, string channel = null)
        {
            string prefix = await _servers.GetGuildPrefix(Context.Guild.Id);

            if (option1 == "mod")
            {
                if (option2 == null)
                {
                    var fetchedChannelId = await _servers.GetModLogsAsync(Context.Guild.Id);
                    if (fetchedChannelId == 0)
                    {
                        await Context.Channel.SendErrorAsync("Mod Logs", "There has not been set a moderation logs channel yet.");
                        return;
                    }

                    var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                    if (fetchedChannel == null)
                    {
                        await Context.Channel.SendErrorAsync("Mod Logs", "There has not been set a moderation logs channel yet.");
                        await _servers.ClearModLogsAsync(Context.Guild.Id);
                        return;
                    }

                    await Context.Channel.SendInfoAsync("Mod Logs", $"The channel used for moderation logs is {fetchedChannel.Mention}.");
                    return;
                }

                if (option2 == "set")
                {
                    if (channel == null)
                    {
                        await Context.Channel.SendErrorAsync("Mod Logs", "Please pass a channel to be set as moderation logs.");
                        return;
                    }

                    if (!MentionUtils.TryParseChannel(channel, out ulong parsedId))
                    {
                        await Context.Channel.SendErrorAsync("Mod Logs", "Please pass a valid channel.");
                        return;
                    }

                    var parsedChannel = Context.Guild.GetTextChannel(parsedId);
                    if (parsedChannel == null)
                    {
                        await Context.Channel.SendErrorAsync("Mod Logs", "Please pass a valid channel.");
                        return;
                    }

                    await _servers.ModifyModLogsAsync(Context.Guild.Id, parsedId);
                    await Context.Channel.SendSuccessAsync("Mod Logs", $"Successfully modified the moderation logs channel to {parsedChannel.Mention}.");
                    return;
                }
                if (option2 == "clear")
                {
                    await _servers.ClearModLogsAsync(Context.Guild.Id);
                    await Context.Channel.SendSuccessAsync("Mod Logs", "Successfully removed the moderation logs channel.");
                    return;
                }

                await Context.Channel.SendInfoAsync("Mod Logs", $"Enter a valid option for the subcommand.\n> {prefix}logs mod set <channel> - to set a new mute announcing channel\n> {prefix}logs mod clear - in order to remove the previous set channel");
            }

            await Context.Channel.SendInfoAsync("Logging", $"Enter a valid subcommand.\n> {prefix}logs mod - to makes changes in the moderation logs channel\n> [Unavailable] {prefix}logs audit -to makes changes in the audit logs channel");
        }

        [Command("inviteblocker", RunMode = RunMode.Async)]
        [Alias("inviteblock", "antiinvite")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task InviteBlocker(string option = null, string value = null)
        {
            IMessage reply;
            var prefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? _config["prefix"];

            if (option == null)
            {
                if (await _modSettings.GetInviteBlockerToggleAsync(Context.Guild.Id) == false)
                {
                    reply = await Context.Channel.SendInfoAsync("Invite Blocker", $"The invite blocker for this server is __currently__ turned **__off__**.\nYou can change this by typing __{prefix}inviteblocker toggle on__.");
                    await Task.Delay(15000);
                    await reply.DeleteAsync();
                    return;
                }
                else
                {
                    reply = await Context.Channel.SendInfoAsync("Invite Blocker", $"The invite blocker for this server is __currently__ turned **__on__**.\nThe punishment level is set to {await _modSettings.GetInviteBlockerSettingAsync(Context.Guild.Id)}.\nYou can change this by typing __{prefix}inviteblocker toggle off__.");
                    await Task.Delay(15000);
                    await reply.DeleteAsync();
                    return;
                }
            }
            if (option == "toggle")
            {
                if (value == null)
                {
                    if (await _modSettings.GetInviteBlockerToggleAsync(Context.Guild.Id) == false)
                    {
                        reply = await Context.Channel.SendInfoAsync("Invite Blocker", "The invite blocker for this server is __currently__ turned **__off__**.");
                        await Task.Delay(7500);
                        await reply.DeleteAsync();
                        return;
                    }
                    else
                    {
                        reply = await Context.Channel.SendInfoAsync("Invite Blocker", "The invite blocker for this server is __currently__ turned **__on__**.");
                        await Task.Delay(7500);
                        await reply.DeleteAsync();
                        return;
                    }
                }
                if (value == "true" || value == "on")
                {
                    if (await _modSettings.GetInviteBlockerToggleAsync(Context.Guild.Id) == true)
                    {
                        reply = await Context.Channel.SendErrorAsync("Invite Blocker", "The invite blocker for this server is __already__ turned **__on__**.");
                        await Task.Delay(7500);
                        await reply.DeleteAsync();
                        return;
                    }
                    else
                    {
                        await _modSettings.ModifyInviteBlockerToggleAsync(Context.Guild.Id, true);
                        reply = await Context.Channel.SendSuccessAsync("Invite Blocker", "The invite blocker for this server is __now__ turned **__on__**.");
                        await Task.Delay(7500);
                        await reply.DeleteAsync();
                        return;
                    }
                }
                if (value == "false" || value == "off")
                {
                    if (await _modSettings.GetInviteBlockerToggleAsync(Context.Guild.Id) == false)
                    {
                        reply = await Context.Channel.SendErrorAsync("Invite Blocker", "The invite blocker for this server is __already__ turned **__off__**.");
                        await Task.Delay(7500);
                        await reply.DeleteAsync();
                        return;
                    }
                    else
                    {
                        await _modSettings.ModifyInviteBlockerToggleAsync(Context.Guild.Id, true);
                        reply = await Context.Channel.SendSuccessAsync("Invite Blocker", "The invite blocker for this server is __now__ turned **__off__**.");
                        await Task.Delay(7500);
                        await reply.DeleteAsync();
                        return;
                    }
                }
            }
            if (option == "punishlevel")
            {
                try
                {
                    int level = Convert.ToInt32(value);

                    switch (level)
                    {
                        case 1:
                            await _modSettings.ModifyInviteBlockerSettingAsync(Context.Guild.Id, level);
                            reply = await Context.Channel.SendSuccessAsync("Invite Blocker", "The punishment level is now set to **1**.");
                            await Task.Delay(7500);
                            await reply.DeleteAsync();
                            return;
                        case 2:
                            await _modSettings.ModifyInviteBlockerSettingAsync(Context.Guild.Id, level);
                            reply = await Context.Channel.SendSuccessAsync("Invite Blocker", "The punishment level is now set to **2**.");
                            await Task.Delay(7500);
                            await reply.DeleteAsync();
                            return;
                        case 3:
                            await _modSettings.ModifyInviteBlockerSettingAsync(Context.Guild.Id, level);
                            reply = await Context.Channel.SendSuccessAsync("Invite Blocker", "The punishment level is now set to **3**.");
                            await Task.Delay(7500);
                            await reply.DeleteAsync();
                            return;
                        case 4:
                            await _modSettings.ModifyInviteBlockerSettingAsync(Context.Guild.Id, level);
                            reply = await Context.Channel.SendSuccessAsync("Invite Blocker", "The punishment level is now set to **4**.");
                            await Task.Delay(7500);
                            await reply.DeleteAsync();
                            return;
                        default:
                            reply = await Context.Channel.SendErrorAsync("Invite Blocker", "Input a valid number for the punishment level.\n>1: Message Delete\n>2: 30 minutes mute\n>3: 2 hours mute\n>4: Guild ban");
                            await Task.Delay(7500);
                            await reply.DeleteAsync();
                            return;
                    }
                }
                catch
                {
                    reply = await Context.Channel.SendErrorAsync("Invite Blocker", "Input a valid number for the punishment level.");
                    await Task.Delay(7500);
                    await reply.DeleteAsync();
                    return;
                }
            }
            reply = await Context.Channel.SendErrorAsync("Invite Blocker", $"Input a valid subcommand:\n>{prefix}inviteblocker toggle\n>{prefix}inviteblocker punishlevel");
            await Task.Delay(7500);
            await reply.DeleteAsync();
            return;
        }

        [Command("slowmode", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task Slowmode(int interval)
        {
            await (Context.Channel as SocketTextChannel).ModifyAsync(x => x.SlowModeInterval = interval);
            var reply = Context.Channel.SendSuccessAsync("Slowmode Changed", $"{(Context.Channel as SocketTextChannel).Mention} slowmode has been set to {interval}");
        }
    }
}
