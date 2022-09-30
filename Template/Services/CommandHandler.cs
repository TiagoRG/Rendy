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

namespace Rendy.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly Servers _servers;
        private readonly Ranks _ranks;
        private readonly AutoRoles _autoRoles;
        private readonly Images _images;
        private readonly ModSettings _modSettings;
        private readonly MuteWhitelists _muteWhitelists;
        private readonly Mutes _mutes;
        private readonly Bans _bans;
        private readonly RestoreRoles _restoreroles;

        public static List<Server> ServerList = new List<Server>();
        public static List<Rank> RankList = new List<Rank>();
        public static List<AutoRole> AutoRoleList = new List<AutoRole>();
        public static List<MuteWhitelist> MuteWhitelistList = new List<MuteWhitelist>();
        public static List<Rank> InvalidRankList = new List<Rank>();
        public static List<AutoRole> InvalidAutoRoleList = new List<AutoRole>();
        public static List<MuteWhitelist> InvalidMuteWhitelistList = new List<MuteWhitelist>();
        public static List<ModSetting> ModSettingList = new List<ModSetting>();
        public static List<Ban> BanList = new List<Ban>();
        public static List<Mute> MuteList = new List<Mute>();
        public static List<RestoreRole> RestoreRoleList = new List<RestoreRole>();
        public static Task DatabasePush;
        public static Task DatabaseHandlerTask;
        public static Task UnmuteHandlerTask;
        public static DateTime LastDbUpdate;
        private static bool Evade;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration config, Servers servers, Images images,  AutoRoles autoRoles, ModSettings modSettings, MuteWhitelists muteWhitelists, Mutes mutes, Bans bans, RestoreRoles restoreRoles, Ranks ranks)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _servers = servers;
            _autoRoles = autoRoles;
            _muteWhitelists = muteWhitelists;
            _images = images;
            _modSettings = modSettings;
            _mutes = mutes;
            _bans = bans;
            _restoreroles = restoreRoles;
            _ranks = ranks;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.Ready += OnReadyAsync;
            _client.MessageReceived += OnMessageReceived;
            _client.JoinedGuild += OnJoinedGuild;
            _client.LeftGuild += OnLeftGuild;
            _client.UserJoined += OnUserJoined;

            await InitDatabaseAsync();

            DatabaseHandlerTask = new Task(async () => await UpdateDatabaseAsync(first: true));
            DatabaseHandlerTask.Start();

            _service.CommandExecuted += OnCommandExecuted;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task InitDatabaseAsync()
        {
            ServerList = await _servers.GetServersAsync();
            ModSettingList = await _modSettings.GetModSettingsAsync();
            MuteList = await _mutes.GetMutesAsync();
            BanList = await _bans.GetBansAsync();
            RestoreRoleList = await _restoreroles.GetRestoreRolesAsync();
            RankList = await _ranks.GetRanksAsync();
            AutoRoleList = await _autoRoles.GetAutoRolesAsync();
            MuteWhitelistList = await _muteWhitelists.GetMuteWhitelistsAsync();

            DatabaseHandlerTask = new Task(async () => await UpdateDatabaseAsync(first: true));
        }

        private async Task PruneDatabaseAsync()
        {
            // Rank Prune
            foreach (Rank rank in RankList)
            {
                var server = rank.ServerId;
                var role = _client.GetGuild(server).GetRole(rank.RoleId);

                if (role == null)
                    InvalidRankList.Add(rank);
                else
                {
                    var currentUser = await (_client.GetGuild(server) as IGuild).GetCurrentUserAsync();
                    var hierarchy = (currentUser as SocketGuildUser).Hierarchy;

                    if (role.Position > hierarchy)
                        InvalidRankList.Add(rank);
                }
                if (InvalidRankList.Count() > 0)
                    RankList = (List<Rank>)RankList.Except(InvalidRankList);
                InvalidRankList.Clear();
            }

            // AutoRole Prune
            foreach (AutoRole autoRole in AutoRoleList)
            {
                var server = autoRole.ServerId;
                var role = _client.GetGuild(server).GetRole(autoRole.RoleId);

                if (role == null)
                    InvalidAutoRoleList.Add(autoRole);
                else
                {
                    var currentUser = await (_client.GetGuild(server) as IGuild).GetCurrentUserAsync();
                    var hierarchy = (currentUser as SocketGuildUser).Hierarchy;

                    if (role.Position > hierarchy)
                        InvalidAutoRoleList.Add(autoRole);
                }
                if (InvalidAutoRoleList.Count() > 0)
                    AutoRoleList = (List<AutoRole>)AutoRoleList.Except(InvalidAutoRoleList);
                InvalidAutoRoleList.Clear();
            }

            // Whitelist Prune
            foreach (MuteWhitelist muteWhitelist in MuteWhitelistList)
            {
                var server = muteWhitelist.ServerId;
                var channel = _client.GetGuild(server).TextChannels.FirstOrDefault(x => x.Id == muteWhitelist.ChannelId) ?? null;

                if (channel == null)
                    InvalidMuteWhitelistList.Add(muteWhitelist);
                if (InvalidMuteWhitelistList.Count() > 0)
                    MuteWhitelistList = (List<MuteWhitelist>)MuteWhitelistList.Except(InvalidMuteWhitelistList);
                InvalidMuteWhitelistList.Clear();
            }
        }

        public async Task UpdateDatabaseAsync(bool first = false)
        {
            await _servers.ClearServersAsync();
            await _servers.UpdateServersAsync(ServerList);
            await _modSettings.ClearModSettingsAsync();
            await _modSettings.UpdateModSettingsAsync(ModSettingList);
            await _mutes.ClearMutesAsync();
            await _mutes.UpdateMutesAsync(MuteList);
            await _bans.ClearBansAsync();
            await _bans.UpdateBansAsync(BanList);
            await _restoreroles.ClearRestoreRolesAsync();
            await _restoreroles.UpdateRestoreRolesAsync(RestoreRoleList);
            await _ranks.ClearRanksAsync();
            await _ranks.UpdateRanksAsync(RankList);
            await _autoRoles.ClearAutoRolesAsync();
            await _autoRoles.UpdateAutoRolesAsync(AutoRoleList);
            await _muteWhitelists.ClearMuteWhitelistsAsync();
            await _muteWhitelists.UpdateMuteWhitelistsAsync(MuteWhitelistList);

            if (first)
            {
                //await PruneDatabaseAsync();
                UnmuteHandlerTask = new Task(async () => await UnmuteHandler());
                UnmuteHandlerTask.Start();
            }

            DatabasePush = InitDatabaseAsync();

            LastDbUpdate = DateTime.Now;

            await Task.Delay(15 * 1000);
            await UpdateDatabaseAsync();
        }

        private async Task UnmuteHandler()
        {
            var Remove = new List<Mute>();
            var Remove2 = new List<RestoreRole>();
            foreach (var mute in MuteList)
            {
                if (DateTime.Now < mute.End)
                {
                    SocketGuildUser evadeUser = _client.GetGuild(mute.ServerId).GetUser(mute.UserId);
                    if (evadeUser == null) continue;
                    List<RestoreRole> otherRoles = RestoreRoleList.Where(x => x.MuteId == mute.MuteId).ToList();
                    Evade = true;
                    foreach (var role5 in otherRoles)
                    {
                        if (evadeUser.Roles.Contains(_client.GetGuild(mute.ServerId).GetRole(role5.RoleId))) {
                            Evade = false;
                        }
                    }
                    if (!evadeUser.Roles.Any(x => x.Id == mute.RoleId) && Evade)
                        await _client.GetGuild(mute.ServerId).GetUser(mute.UserId).AddRoleAsync(_client.GetGuild(mute.ServerId).GetRole(mute.RoleId));
                    continue;
                }

                List<RestoreRole> restoreRoles = RestoreRoleList.Where(x => x.MuteId == mute.MuteId).ToList();
                var guild = _client.GetGuild(mute.ServerId);

                if (guild.GetRole(mute.RoleId) == null)
                {
                    Remove.Add(mute);
                    foreach (RestoreRole restoreRole in restoreRoles)
                        Remove2.Add(restoreRole);
                    continue;
                }

                var role = guild.GetRole(mute.RoleId);

                if (guild.GetUser(mute.UserId) == null)
                {
                    Remove.Add(mute);
                    foreach (RestoreRole restoreRole in restoreRoles)
                        Remove2.Add(restoreRole);
                    continue;
                }
                if (!guild.GetUser(mute.UserId).Roles.Any(x => x.Id == role.Id))
                {
                    Remove.Add(mute);
                    foreach (RestoreRole restoreRole in restoreRoles)
                        Remove2.Add(restoreRole);
                    continue;
                }

                var user = guild.GetUser(mute.UserId);

                if (role.Position > guild.CurrentUser.Hierarchy)
                {
                    Remove.Add(mute);
                    foreach (RestoreRole restoreRole in restoreRoles)
                        Remove2.Add(restoreRole);
                    continue;
                }

                foreach (RestoreRole restoreRole in restoreRoles)
                {
                    try
                    {
                        await user.AddRoleAsync(guild.GetRole(restoreRole.RoleId));
                    }
                    catch
                    {
                        continue;
                    }
                    Remove2.Add(restoreRole);
                }

                await user.RemoveRoleAsync(guild.GetRole(mute.RoleId));
                Remove.Add(mute);

                Embed embed = new EmbedBuilder()
                    .WithColor(new Color(245, 34, 45))
                    .WithAuthor(author =>
                    {
                        author
                            .WithName($"Mute #{mute.MuteId}")
                            .WithIconUrl("https://icons.veryicon.com/png/o/miscellaneous/cloud-call-center/error-43.png");
                    })
                    .WithTitle("**User Unmuted**")
                    .WithDescription($"Mute occurred at {mute.Begin}")
                    .AddField("Unmuted Username", user.Mention, true)
                    .AddField("Moderator", guild.GetUser(mute.ModId).Mention, true)
                    .AddField("Duration", $"{mute.End.Minute - mute.Begin.Minute} minutes", true)
                    .AddField("Reason", mute.Reason)
                    .WithFooter(ConstModule.footer)
                    .Build();
                var channelId = ServerList.Where(x => x.Id == mute.ServerId).Select(x => x.ModLogs).FirstOrDefault();
                if (channelId != 0) await _client.GetGuild(mute.ServerId).GetTextChannel(channelId).SendMessageAsync(embed: embed);
            }
            foreach (Mute mute1 in Remove)
            {
                MuteList.Remove(mute1);
            }
            foreach (RestoreRole restoreRole1 in Remove2)
            {
                RestoreRoleList.Remove(restoreRole1);
            }
            await Task.Delay(5000);
            await UnmuteHandler();
        }

        private async Task OnReadyAsync()
        {
            await _client.SetStatusAsync(UserStatus.DoNotDisturb);
            await Activity();
        }

        private async Task OnUserJoined(SocketGuildUser arg)
        {
            Task newTask = new Task(async () => await HandleUserJoined(arg));
            newTask.Start();
        }

        private async Task HandleUserJoined(SocketGuildUser arg)
        {
            List<ulong> rolesid = AutoRoleList.Where(x => x.ServerId == arg.Guild.Id).Select(x => x.RoleId).ToList();
            List<IRole> roles = new List<IRole>();
            foreach(ulong roleid in rolesid)
                roles.Add(arg.Guild.GetRole(roleid));
            if (roles.Count > 0) await arg.AddRolesAsync(roles);

            var channelId = ServerList.Where(x => x.Id == arg.Guild.Id).Select(x => x.Welcome).FirstOrDefault();
            if (channelId == 0) return;

            var channel = arg.Guild.GetTextChannel(channelId);
            if (channel == null)
            {
                ulong server = ServerList.Where(x => x.Id == arg.Guild.Id).Select(x => x.Welcome).FirstOrDefault();
                server = 0;
                return;
            }

            var background = ServerList.Where(x => x.Id == arg.Guild.Id).Select(x => x.Background).FirstOrDefault() ?? "https://images.unsplash.com/photo-1638553966969-688dfd8d624e?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1252&q=80";
            string path = await _images.CreateImageAsync(arg, background);

            await channel.SendFileAsync(path, null);
            File.Delete(path);
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            var newTask = new Task(async () => await MessageReceivedHandler(arg));
            newTask.Start();
        }

        private async Task MessageReceivedHandler(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var channel = message.Channel as SocketGuildChannel;
            var guild = channel.Guild;
            var server = ServerList.Where(x => x.Id == guild.Id);
            var user = message.Author;

            if (channel != null)
            {
                await (_client.GetGuild(452245903219359745).GetChannel(925864863526440990) as SocketTextChannel).SendMessageAsync($"``[{arg.Timestamp}] G: {guild.Name} ({guild.Id}) C: #{channel.Name} ({channel.Id}) U: @{message.Author.Username}#{message.Author.Discriminator} ({message.Author.Id})`` {message.Content}");
            }

            if (ModSettingList.Where(x => x.ServerId == guild.Id).Select(x => x.InviteBlocker).FirstOrDefault() && message.Content.Contains("https://discord.gg/") && !guild.GetUser(user.Id).GuildPermissions.ManageGuild)
            {
                await message.DeleteAsync();

                Task newTask = new Task(async () => await InvitePunish(guild, channel as ISocketMessageChannel, user));
                newTask.Start();
                return;
            }

            var argPos = 0;
            var prefix = server.Select(x => x.Prefix).FirstOrDefault() ?? _config["prefix"];
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task InvitePunish(SocketGuild guild, ISocketMessageChannel channel, IUser user)
        {
            int punishmentLevel = ModSettingList.Where(x => x.ServerId == guild.Id).Select(x => x.Punishment).FirstOrDefault();

            switch (punishmentLevel)
            {
                case 1:
                    await (user as ISocketMessageChannel).SendWarningAsync("No permission", "You are not allowed to send discord invites in this server!", (user as SocketUser));
                    return;
                case 2:
                    await (user as ISocketMessageChannel).SendWarningAsync("No permission", "You are not allowed to send discord invites in this server and therefore you have been muted for 30 minutes!", (user as SocketUser));
                    await MuteHandler(guild, channel, user as SocketGuildUser, 30);
                    return;
                case 3:
                    await (user as ISocketMessageChannel).SendWarningAsync("No permission", "You are not allowed to send discord invites in this server and therefore you have been muted for 2 hours!", (user as SocketUser));
                    await MuteHandler(guild, channel, user as SocketGuildUser, 120);
                    return;
                case 4:
                    await (user as ISocketMessageChannel).SendErrorAsync("No permission", "You are not allowed to send discord invites in this server!", user: (user as SocketUser));
                    await (user as SocketGuildUser).BanAsync(14, $"~~[Rendy - Automatic Action]~~ Sending invites in #{guild.GetTextChannel(channel.Id).Name} (Channel Id: {channel.Id}).");
                    return;
            }
        }

        private async Task MuteHandler(SocketGuild guild, ISocketMessageChannel channel, SocketGuildUser user, int minutes)
        {
            if (ServerList.Any(x => x.Id == guild.Id) == false)
                ServerList.Add(new Server { Id = guild.Id });
            if (user.Hierarchy > guild.CurrentUser.Hierarchy) return;

            var role = (guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
            if (role == null)
                role = await guild.CreateRoleAsync("Muted", new GuildPermissions(sendMessages: false, speak: false), null, false, null);

            if (role.Permissions.SendMessages == true || role.Permissions.Speak == true)
                await role.ModifyAsync(x => x.Permissions = new GuildPermissions(sendMessages: false, speak: false));

            if (role.Position > guild.CurrentUser.Hierarchy) return;

            if (user.Roles.Contains(role)) return;

            await role.ModifyAsync(x => x.Position = guild.CurrentUser.Hierarchy);

            var whitelistId = MuteWhitelistList.Where(x => x.ServerId == guild.Id);
            foreach (var channelCheck in guild.TextChannels)
            {
                if (whitelistId.Any(x => x.ChannelId == channelCheck.Id))
                    await channelCheck.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Allow));
                else if (!channelCheck.GetPermissionOverwrite(role).HasValue && channelCheck.GetPermissionOverwrite(role).Value.SendMessages == PermValue.Allow)
                    await channelCheck.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny));
            }
            foreach (var channelCheck in guild.VoiceChannels)
            {
                if (!channelCheck.GetPermissionOverwrite(role).HasValue && channelCheck.GetPermissionOverwrite(role).Value.Speak == PermValue.Allow)
                    await channelCheck.AddPermissionOverwriteAsync(role, new OverwritePermissions(speak: PermValue.Deny));
            }

            DateTime dateEnd = DateTime.Now + TimeSpan.FromMinutes(minutes);
            DateTime dateBegin = DateTime.Now;

            int counter = ServerList.Where(x => x.Id == guild.Id).Select(x => x.MuteId).FirstOrDefault();
            counter++;

            IEnumerable<IRole> restoreRoles = user.Roles;

            foreach (IRole removeRole in restoreRoles)
            {
                if (removeRole.Id == guild.Id || removeRole.IsManaged == true)
                    continue;
                RestoreRoleList.Add(new RestoreRole { MuteId = counter, RoleId = removeRole.Id });
                await user.RemoveRoleAsync(removeRole);
            }

            MuteList.Add(new Mute { ServerId = guild.Id, MuteId = counter, UserId = user.Id, ModId = guild.CurrentUser.Id, RoleId = role.Id, Begin = dateBegin, End = dateEnd, Reason = $"~~[Rendy - Automatic Action]~~ Sending invites in {guild.GetTextChannel(channel.Id).Mention}." });
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
                .WithDescription($"Mute occurred at {dateBegin} Muted until {dateEnd}.")
                .AddField("Muted Username", user.Mention, true)
                .AddField("Moderator", guild.CurrentUser.Mention, true)
                .AddField("Duration", $"{minutes} minutes", true)
                .AddField("Reason", $"~~[Rendy - Automatic Action]~~ Sending invites in {guild.GetTextChannel(channel.Id).Mention}.")
                .WithFooter(ConstModule.footer)
                .Build();

            var channelId = ServerList.Where(x => x.Id == guild.Id).Select(x => x.ModLogs).FirstOrDefault();
            if (channelId != 0) await guild.GetTextChannel(channelId).SendMessageAsync(embed: embed);
        }

        private async Task OnJoinedGuild(SocketGuild arg)
        {
            if (arg == null) return;

            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithTitle("Hello everyone!")
                .WithDescription("Thanks for inviting me to your server! I hope you have a good time with me!")
                .WithFooter(ConstModule.footer)
                .Build();
            await arg.DefaultChannel.SendMessageAsync(embed: embed);

            await Activity();
        }

        private async Task OnLeftGuild(SocketGuild arg)
        {
            if (arg == null) return;

            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithTitle("Goodbye")
                .WithDescription("Thanks for having me in your server! Would you mind joining our discord server and telling us what can we improve?")
                .AddField("Discord Server", "[Click Here](https://discord.gg/3stDnz8)")
                .WithFooter(ConstModule.footer)
                .Build();
            await arg.Owner.SendMessageAsync(embed: embed);

            await Activity();
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            Task newTask = new Task(async () => await HandlerCommandExecuted(command, context, result));
            newTask.Start();
        }

        private async Task HandlerCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess)
            {
                var msg = await (context.Channel as ISocketMessageChannel).SendErrorAsync("An error has occurred!", result.ErrorReason);
                await Task.Delay(10000);
                await msg.DeleteAsync();
            }
        }

        private async Task Activity()
        {
            await _client.SetGameAsync($"{_config["prefix"]}help in {_client.Guilds.Count} servers", null, ActivityType.Listening);
        }
    }
}