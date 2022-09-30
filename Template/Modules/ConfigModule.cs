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
using Rendy.Services;

namespace Rendy.Modules
{
    public class ConfigModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _config;

        public ConfigModule(IConfiguration config)
        {
            _config = config;
        }

        [Command("ranks", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task Ranks()
        {
            var ranks = CommandHandler.RankList.Where(x => x.ServerId == Context.Guild.Id);
            var guildPrefix = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.Prefix).FirstOrDefault() ?? _config["prefix"];
            if (ranks.Count() == 0)
            {
                await ReplyAsync($"This server has no ranks yet, use {guildPrefix}addrank in order to set up a rank");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string rankList = "";

            foreach(var rank in ranks)
            {
                var role = Context.Guild.GetRole(rank.RoleId);
                if (role == null) continue;
                rankList += $"\n{role.Mention} ({rank.Id})";
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

        [Command("addrank", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = CommandHandler.RankList.Where(x => x.ServerId == Context.Guild.Id);

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

            if (ranks.Any(x => x.RoleId == role.Id))
            {
                await ReplyAsync("That role is already a rank.");
                return;
            }

            CommandHandler.RankList.Add(new Rank { ServerId = Context.Guild.Id, RoleId = role.Id });
            await ReplyAsync($"The role {role.Mention} has been added to the ranks.");
        }

        [Command("delrank", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = CommandHandler.RankList.Where(x => x.ServerId == Context.Guild.Id);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist. (Use the name of the role, don't mention it)");
                return;
            }

            if (ranks.Any(x => x.RoleId != role.Id))
            {
                await ReplyAsync("That role is not a rank yet.");
                return;
            }

            Rank Remove = new Rank { ServerId = Context.Guild.Id, RoleId = role.Id };
            CommandHandler.RankList.Remove(Remove);
            await ReplyAsync($"The role {role.Mention} has been removed from the ranks.");
        }

        [Command("autoroles", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AutoRoles()
        {
            var autoRoles = CommandHandler.AutoRoleList.Where(x => x.ServerId == Context.Guild.Id);
            if (autoRoles.Count() == 0)
            {
                await ReplyAsync("This server does not yet have any autoroles!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string autoRoleList = "";
            foreach (var autoRole in autoRoles)
            {
                autoRoleList += $"{Context.Guild.GetRole(autoRole.RoleId).Mention}";
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
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = CommandHandler.AutoRoleList.Where(x => x.ServerId == Context.Guild.Id);

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

            if (autoRoles.Any(x => x.RoleId == role.Id))
            {
                await ReplyAsync("That role is already an autorole!");
                return;
            }

            CommandHandler.AutoRoleList.Add(new AutoRole { ServerId = Context.Guild.Id, RoleId = role.Id });
            await ReplyAsync($"The role {role.Mention} has been added to the autoroles!");
        }

        [Command("delautorole", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = CommandHandler.AutoRoleList.Where(x => x.ServerId == Context.Guild.Id);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }

            if (autoRoles.Any(x => x.RoleId != role.Id))
            {
                await ReplyAsync("That role is not an autorole yet!");
                return;
            }

            AutoRole Remove = new AutoRole { ServerId = Context.Guild.Id, RoleId = role.Id };
            CommandHandler.AutoRoleList.Remove(Remove);
            await ReplyAsync($"The role {role.Mention} has been removed from the autoroles!");
        }

        [Command("prefix", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task Prefix(string prefix = null)
        {
            var server = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id);
            var currentPrefix = server.Select(x => x.Prefix).FirstOrDefault();
            if (prefix == null)
            {
                var guildPrefix = currentPrefix ?? _config["prefix"];
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
            if (prefix == currentPrefix)
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

            var embed2 = new EmbedBuilder()
                    .WithAuthor("Rendy")
                    .WithTitle("Guild Prefix")
                    .WithDescription("My prefix for this server have been updated successfully!")
                    .AddField("Previous prefix", $"**{currentPrefix ?? _config["prefix"]}**")
                    .AddField("New prefix", $"**{prefix}**")
                    .WithColor((Discord.Color)ColorTranslator.FromHtml("#747474"))
                    .WithFooter(ConstModule.footer)
                    .Build();
            if (!CommandHandler.ServerList.Any(x => x.Id == Context.Guild.Id))
                CommandHandler.ServerList.Add(new Server { Id = Context.Guild.Id, Prefix = prefix });
            else
                server.ToList().ForEach(y => y.Prefix = prefix);        
            await Context.Channel.SendMessageAsync(embed: embed2);
        }

        [Command("welcome", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task Welcome(string option = null, string value = null)
        {
            var server = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id);
            if (server == null)
                CommandHandler.ServerList.Add(new Server { Id = Context.Guild.Id });
            var fetchedChannelId = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.Welcome).FirstOrDefault();
            var fetchedBackground = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.Background).FirstOrDefault();
            if (option == null && value == null)
            {
                if (fetchedChannelId == 0)
                {
                    await ReplyAsync("There has not been set a welcome channel yet.");
                    return;
                }

                var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                if (fetchedChannel == null)
                {
                    await ReplyAsync("There has not been set a welcome channel yet.");
                    server.ToList().ForEach(x => x.Welcome = 0);
                    return;
                }

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

                server.ToList().ForEach(x => x.Welcome = parsedId);
                await ReplyAsync($"Successfully modified the welcome channel to {parsedChannel.Mention}.");
                return;
            }
            if (option == "background" && value != null)
            {
                if (value == "clear")
                {
                    server.ToList().ForEach(x => x.Background = null);
                    await ReplyAsync($"Successfully removed the background for this server.");
                    return;
                }

                server.ToList().ForEach(x => x.Background = value);
                await ReplyAsync($"Successfully modified the background to {value}.");
                return;
            }
            if (option == "clear")
            {
                server.ToList().ForEach(x => x.Welcome = 0);
                await ReplyAsync("Successfully removed the welcome channel.");
                return;
            }

            await ReplyAsync("You did not use this command properly.");
        }

        [Command("mutewhitelist", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task MuteWhitelist(string option = null, string channel = null)
        {
            string prefix = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.Prefix).FirstOrDefault();
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

                var whitelist = CommandHandler.MuteWhitelistList.Where(x => x.ServerId == Context.Guild.Id);
                if (whitelist.Any(x => x.ChannelId == parsedChannel.Id))
                {
                    await Context.Channel.SendErrorAsync("Mute Whitelist", "That channel is already in the whitelist.");
                    return;
                }

                CommandHandler.MuteWhitelistList.Add(new MuteWhitelist { ServerId = Context.Guild.Id, ChannelId = parsedChannel.Id });
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

                var whitelist = CommandHandler.MuteWhitelistList.Where(x => x.ServerId == Context.Guild.Id);
                if (!whitelist.Any(x => x.ChannelId == parsedChannel.Id))
                {
                    await Context.Channel.SendErrorAsync("Mute Whitelist", "That channel is not in the whitelist.");
                    return;
                }

                MuteWhitelist Remove = new MuteWhitelist { ServerId = Context.Guild.Id, ChannelId = parsedChannel.Id };
                CommandHandler.MuteWhitelistList.Remove(Remove);
                await Context.Channel.SendSuccessAsync("Mute Whitelist", $"Successfully removed {parsedChannel.Mention} from the whitelist.");
                return;
            }

            await Context.Channel.SendInfoAsync("Mute Whitelist", $"Enter a valid option for the subcommand.\n> {prefix}whitelist add <channel> - to add a new channel to the whitelist\n> {prefix}whitelist remove <channel> - to remove 1 channel from the whitelist");
        }

        [Command("logs", RunMode = RunMode.Async)]
        [Alias("log", "logging")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Logs(string option1 = null, string option2 = null, string channel = null)
        {
            if (CommandHandler.ServerList.Any(x => x.Id == Context.Guild.Id) == false)
                CommandHandler.ServerList.Add(new Server { Id = Context.Guild.Id });
            var server = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id);
            string prefix = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.Prefix).FirstOrDefault() ?? _config["prefix"];
            ulong modLogs = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.ModLogs).FirstOrDefault();
            ulong auditLogs = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.AuditLogs).FirstOrDefault();

            if (option1 == "mod")
            {
                if (option2 == null)
                {
                    var fetchedChannelId = modLogs;
                    if (fetchedChannelId == 0)
                    {
                        await Context.Channel.SendErrorAsync("Mod Logs", "There has not been set a moderation logs channel yet.");
                        return;
                    }

                    var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                    if (fetchedChannel == null)
                    {
                        await Context.Channel.SendErrorAsync("Mod Logs", "There has not been set a moderation logs channel yet.");
                        server.ToList().ForEach(x => x.ModLogs = 0);
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

                    server.ToList().ForEach(x => x.ModLogs = parsedId);
                    await Context.Channel.SendSuccessAsync("Mod Logs", $"Successfully modified the moderation logs channel to {parsedChannel.Mention}.");
                    return;
                }
                if (option2 == "clear")
                {
                    server.ToList().ForEach(x => x.ModLogs = 0);
                    await Context.Channel.SendSuccessAsync("Mod Logs", "Successfully removed the moderation logs channel.");
                    return;
                }

                await Context.Channel.SendInfoAsync("Mod Logs", $"Enter a valid option for the subcommand.\n> {prefix}logs mod set <channel> - to set a new mute announcing channel\n> {prefix}logs mod clear - in order to remove the previous set channel");
            }
            if (option1 == "audit")
            {
                if (option2 == null)
                {
                    var fetchedChannelId = auditLogs;
                    if (fetchedChannelId == 0)
                    {
                        await Context.Channel.SendErrorAsync("Audit Logs", "There has not been set a audit logs channel yet.");
                        return;
                    }

                    var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                    if (fetchedChannel == null)
                    {
                        await Context.Channel.SendErrorAsync("Audit Logs", "There has not been set a audit logs channel yet.");
                        server.ToList().ForEach(x => x.AuditLogs = 0);
                        return;
                    }

                    await Context.Channel.SendInfoAsync("Audit Logs", $"The channel used for audit logs is {fetchedChannel.Mention}.");
                    return;
                }

                if (option2 == "set")
                {
                    if (channel == null)
                    {
                        await Context.Channel.SendErrorAsync("Audit Logs", "Please pass a channel to be set as audit logs.");
                        return;
                    }

                    if (!MentionUtils.TryParseChannel(channel, out ulong parsedId))
                    {
                        await Context.Channel.SendErrorAsync("Audit Logs", "Please pass a valid channel.");
                        return;
                    }

                    var parsedChannel = Context.Guild.GetTextChannel(parsedId);
                    if (parsedChannel == null)
                    {
                        await Context.Channel.SendErrorAsync("Audit Logs", "Please pass a valid channel.");
                        return;
                    }

                    server.ToList().ForEach(x => x.AuditLogs = parsedId);
                    await Context.Channel.SendSuccessAsync("Audit Logs", $"Successfully modified the audit logs channel to {parsedChannel.Mention}.");
                    return;
                }
                if (option2 == "clear")
                {
                    server.ToList().ForEach(x => x.AuditLogs = 0);
                    await Context.Channel.SendSuccessAsync("Audit Logs", "Successfully removed the audit logs channel.");
                    return;
                }

                await Context.Channel.SendInfoAsync("Audit Logs", $"Enter a valid option for the subcommand.\n> {prefix}logs audit set <channel> - to set a new mute announcing channel\n> {prefix}logs audit clear - in order to remove the previous set channel");
            }

            await Context.Channel.SendInfoAsync("Logging", $"Enter a valid subcommand.\n> {prefix}logs mod - to makes changes in the moderation logs channel\n> {prefix}logs audit - to makes changes in the audit logs channel");
        }

        [Command("inviteblocker", RunMode = RunMode.Async)]
        [Alias("inviteblock", "antiinvite")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task InviteBlocker(string option = null, string value = null)
        {
            if (CommandHandler.ServerList.Any(x => x.Id == Context.Guild.Id) == false)
                CommandHandler.ServerList.Add(new Server { Id = Context.Guild.Id });
            if (CommandHandler.ModSettingList.Any(x => x.ServerId == Context.Guild.Id) == false)
                CommandHandler.ModSettingList.Add(new ModSetting { ServerId = Context.Guild.Id });
            var serverModSettings = CommandHandler.ModSettingList.Where(x => x.ServerId == Context.Guild.Id);
            IMessage reply;
            string prefix = CommandHandler.ServerList.Where(x => x.Id == Context.Guild.Id).Select(x => x.Prefix).FirstOrDefault() ?? _config["prefix"];
            bool inviteBlockerToggle = CommandHandler.ModSettingList.Where(x => x.ServerId == Context.Guild.Id).Select(x => x.InviteBlocker).FirstOrDefault();
            int inviteBlockerPunish = CommandHandler.ModSettingList.Where(x => x.ServerId == Context.Guild.Id).Select(x => x.Punishment).FirstOrDefault();

            if (option == null)
            {
                if (inviteBlockerToggle == false)
                {
                    reply = await Context.Channel.SendInfoAsync("Invite Blocker", $"The invite blocker for this server is __currently__ turned **__off__**.\nYou can change this by typing __{prefix}inviteblocker toggle on__.");
                    await Task.Delay(15000);
                    await reply.DeleteAsync();
                    return;
                }
                else
                {
                    reply = await Context.Channel.SendInfoAsync("Invite Blocker", $"The invite blocker for this server is __currently__ turned **__on__**.\nThe punishment level is set to {inviteBlockerPunish}.\nYou can change this by typing __{prefix}inviteblocker toggle off__.");
                    await Task.Delay(15000);
                    await reply.DeleteAsync();
                    return;
                }
            }
            if (option == "toggle")
            {
                if (value == null)
                {
                    if (inviteBlockerToggle == false)
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
                    if (inviteBlockerToggle == true)
                    {
                        reply = await Context.Channel.SendErrorAsync("Invite Blocker", "The invite blocker for this server is __already__ turned **__on__**.");
                        await Task.Delay(7500);
                        await reply.DeleteAsync();
                        return;
                    }
                    else
                    {
                        serverModSettings.ToList().ForEach(x => x.InviteBlocker = true);
                        reply = await Context.Channel.SendSuccessAsync("Invite Blocker", "The invite blocker for this server is __now__ turned **__on__**.");
                        await Task.Delay(7500);
                        await reply.DeleteAsync();
                        return;
                    }
                }
                if (value == "false" || value == "off")
                {
                    if (inviteBlockerToggle == false)
                    {
                        reply = await Context.Channel.SendErrorAsync("Invite Blocker", "The invite blocker for this server is __already__ turned **__off__**.");
                        await Task.Delay(7500);
                        await reply.DeleteAsync();
                        return;
                    }
                    else
                    {
                        serverModSettings.ToList().ForEach(x => x.InviteBlocker = false);
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
                            serverModSettings.ToList().ForEach(x => x.Punishment = 1);
                            reply = await Context.Channel.SendSuccessAsync("Invite Blocker", "The punishment level is now set to **1**.");
                            await Task.Delay(7500);
                            await reply.DeleteAsync();
                            return;
                        case 2:
                            serverModSettings.ToList().ForEach(x => x.Punishment = 2);
                            reply = await Context.Channel.SendSuccessAsync("Invite Blocker", "The punishment level is now set to **2**.");
                            await Task.Delay(7500);
                            await reply.DeleteAsync();
                            return;
                        case 3:
                            serverModSettings.ToList().ForEach(x => x.Punishment = 3);
                            reply = await Context.Channel.SendSuccessAsync("Invite Blocker", "The punishment level is now set to **3**.");
                            await Task.Delay(7500);
                            await reply.DeleteAsync();
                            return;
                        case 4:
                            serverModSettings.ToList().ForEach(x => x.Punishment = 4);
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
            reply = await Context.Channel.SendErrorAsync("Invite Blocker", $"Input a valid subcommand:\n> {prefix}inviteblocker toggle\n> {prefix}inviteblocker punishlevel");
            await Task.Delay(7500);
            await reply.DeleteAsync();
            return;
        }

        [Command("slowmode", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task Slowmode(int interval)
        {
            await (Context.Channel as SocketTextChannel).ModifyAsync(x => x.SlowModeInterval = interval);
            var reply = Context.Channel.SendSuccessAsync("Slowmode Changed", $"{(Context.Channel as SocketTextChannel).Mention} slowmode has been set to {interval}");
        }
    }
}
