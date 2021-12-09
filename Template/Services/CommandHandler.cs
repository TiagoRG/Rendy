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
using static Database.Mutes;

namespace Rendy.Services
{
    public class MuteHandle
    {
        public int MuteId;
        public ulong ServerId;
        public ulong UserId;
        public ulong ModId;
        public ulong RoleId;
        public List<ulong> RestoreRolesId;
        public DateTime Begin;
        public DateTime End;
        public string Reason;
    }

    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly Servers _servers;
        private readonly AutoRolesHelper _autoRolesHelper;
        private readonly Images _images;
        private readonly ModSettings _modSettings;
        private readonly MuteWhitelistsHelper _muteWhitelistsHelper;
        private readonly Mutes _mutes;
        private readonly RestoreRoles _restoreroles;
        public static List<MuteHandle> MuteHandleList = new List<MuteHandle>();
        public static bool AutoModTaskAvailable;
        public static bool CommandTaskAvailable;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration config, Servers servers, Images images,  AutoRolesHelper autoRolesHelper, ModSettings modSettings, MuteWhitelistsHelper muteWhitelistsHelper, Mutes mutes, RestoreRoles restoreRoles)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _servers = servers;
            _autoRolesHelper = autoRolesHelper;
            _muteWhitelistsHelper = muteWhitelistsHelper;
            _images = images;
            _modSettings = modSettings;
            _mutes = mutes;
            _restoreroles = restoreRoles;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.Ready += OnReadyAsync;
            _client.MessageReceived += OnMessageReceived;
            _client.JoinedGuild += OnJoinedGuild;
            _client.LeftGuild += OnLeftGuild;
            _client.UserJoined += OnUserJoined;

            AutoModTaskAvailable = true;
            CommandTaskAvailable = true;

            var unmuteHandler = new Task(async () => await UnmuteHandler());
            unmuteHandler.Start();

            _service.CommandExecuted += OnCommandExecuted;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task UnmuteHandler()
        {
            while (CommandTaskAvailable == false) { }
            AutoModTaskAvailable = false;
            List<MuteData> MuteList = await _mutes.GetMutesAsync();
            if (MuteList != null)
            {
                foreach (var mute in MuteList)
                {
                    if (DateTime.Now < mute.End)
                    {
                        continue;
                    }

                    var guild = _client.GetGuild(mute.ServerId);

                    if (guild.GetRole(mute.RoleId) == null)
                    {
                        await _mutes.RemoveMuteAsync(mute.ServerId, mute.UserId);
                        continue;
                    }

                    var role = guild.GetRole(mute.RoleId);

                    if (guild.GetUser(mute.UserId) == null)
                    {
                        await _mutes.RemoveMuteAsync(mute.ServerId, mute.UserId);
                        continue;
                    }
                    if (!guild.GetUser(mute.UserId).Roles.Any(x => x.Id == role.Id))
                    {
                        await _mutes.RemoveMuteAsync(mute.ServerId, mute.UserId);
                        continue;
                    }

                    var user = guild.GetUser(mute.UserId);

                    if (role.Position > guild.CurrentUser.Hierarchy)
                    {
                        await _mutes.RemoveMuteAsync(mute.ServerId, mute.UserId);
                        continue;
                    }

                    List<ulong> restoreRolesId = await _restoreroles.GetRestoreRolesIdAsync(mute.MuteId);

                    foreach (ulong restoreRoleId in restoreRolesId)
                    {
                        try
                        {
                            await user.AddRoleAsync(guild.GetRole(restoreRoleId));
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    await user.RemoveRoleAsync(guild.GetRole(mute.RoleId));
                    await _mutes.RemoveMuteAsync(mute.ServerId, mute.UserId);
                    await _restoreroles.RemoveRestoreRolesAsync(mute.MuteId);

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
                        .WithFooter(_config["embedFooter"])
                        .Build();
                    var channelId = await _servers.GetModLogsAsync(mute.ServerId);
                    if (channelId != 0) await _client.GetGuild(mute.ServerId).GetTextChannel(channelId).SendMessageAsync(embed: embed);
                }
            }
            AutoModTaskAvailable = true;
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
            var roles = await _autoRolesHelper.GetAutoRolesAsync(arg.Guild);
            if (roles.Count > 0) await arg.AddRolesAsync(roles);

            var channelId = await _servers.GetWelcomeAsync(arg.Guild.Id);
            if (channelId == 0) return;

            var channel = arg.Guild.GetTextChannel(channelId);
            if (channel == null)
            {
                await _servers.ClearWelcomeAsync(arg.Guild.Id);
                return;
            }

            var background = await _servers.GetBackgroundAsync(arg.Guild.Id);
            string path = await _images.CreateImageAsync(arg, background);

            await channel.SendFileAsync(path, null);
            File.Delete(path);
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var channel = message.Channel as SocketGuildChannel;
            var guild = channel.Guild;

            if (await _modSettings.GetInviteBlockerToggleAsync(guild.Id))
            {
                if (message.Content.Contains("https://discord.gg/"))
                {
                    if (!guild.GetUser(message.Author.Id).GuildPermissions.ManageGuild)
                    {
                        await message.DeleteAsync();

                        Task newTask = new Task(async () => await InvitePunish(guild, channel as ISocketMessageChannel, message.Author));
                        newTask.Start();
                        return;
                    }
                }
            }

            var argPos = 0;
            var prefix = await _servers.GetGuildPrefix(guild.Id) ?? _config["prefix"];
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task InvitePunish(SocketGuild guild, ISocketMessageChannel channel, IUser user)
        {
            int punishmentLevel = await _modSettings.GetInviteBlockerSettingAsync(guild.Id);

            switch (punishmentLevel)
            {
                case 1:
                    await (user as ISocketMessageChannel).SendWarningAsync("No permission", "You are not allowed to send discord invites in this server!", (user as SocketUser));
                    return;
                case 2:
                    await (user as ISocketMessageChannel).SendWarningAsync("No permission", "You are not allowed to send discord invites in this server and therefore you have been muted for 30 minutes!", (user as SocketUser));
                    await MuteHandler(guild, channel, user as SocketGuildUser, 5);
                    return;
                case 3:
                    await (user as ISocketMessageChannel).SendWarningAsync("No permission", "You are not allowed to send discord invites in this server and therefore you have been muted for 2 hours!", (user as SocketUser));
                    await MuteHandler(guild, channel, user as SocketGuildUser, 10);
                    return;
                case 4:
                    await (user as ISocketMessageChannel).SendErrorAsync("No permission", "You are not allowed to send discord invites in this server!", user: (user as SocketUser));
                    await (user as SocketGuildUser).BanAsync(14, $"~~[Rendy - Automatic Action]~~ Sending invites in #{guild.GetTextChannel(channel.Id).Name} (Channel Id: {channel.Id}).");
                    return;
            }
        }

        private async Task MuteHandler(SocketGuild guild, ISocketMessageChannel channel, SocketGuildUser user, int minutes)
        {
            if (user.Hierarchy > guild.CurrentUser.Hierarchy) return;

            var role = (guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
            if (role == null)
                role = await guild.CreateRoleAsync("Muted", new GuildPermissions(sendMessages: false, speak: false), null, false, null);

            if (role.Permissions.SendMessages == true || role.Permissions.Speak == true)
                await role.ModifyAsync(x => x.Permissions = new GuildPermissions(sendMessages: false, speak: false));

            if (role.Position > guild.CurrentUser.Hierarchy) return;

            if (user.Roles.Contains(role)) return;

            await role.ModifyAsync(x => x.Position = guild.CurrentUser.Hierarchy);

            var whitelistId = await _muteWhitelistsHelper.GetMuteWhitelistAsync(guild);
            foreach (var channelCheck in guild.TextChannels)
            {
                if (whitelistId.Any(x => x.Id == channel.Id))
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

            IEnumerable<IRole> restoreRoles = user.Roles;
            List<ulong> restoreRolesId = new List<ulong>();

            foreach (IRole removeRole in restoreRoles)
            {
                if (removeRole.Id == guild.Id || removeRole.IsManaged == true)
                    continue;
                restoreRolesId.Append(removeRole.Id);
                await user.RemoveRoleAsync(removeRole);
            }

            int counter = await _servers.GetMuteCounter(guild.Id);
            await _mutes.AddMuteAsync(guild.Id, counter, user.Id, guild.CurrentUser.Id, role.Id, dateBegin, dateEnd, $"~~[Rendy - Automatic Action]~~ Sending invites in {guild.GetTextChannel(channel.Id).Mention}.");
            int muteId = await _mutes.GetMuteIdAsync(guild.Id, user.Id);
            await _restoreroles.AddRestoreRolesAsync(muteId, restoreRolesId);
            await _servers.AddMuteCounter(guild.Id);
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

            var channelId = await _servers.GetModLogsAsync(guild.Id);
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