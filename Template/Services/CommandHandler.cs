using System;
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
        private readonly Images _images;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration config, Servers servers, Images images)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _servers = servers;
            _images = images;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.Ready += LoggedIn;
            _client.MessageReceived += OnMessageReceived;
            _client.JoinedGuild += OnJoinedGuild;
            _client.UserJoined += OnUserJoined;
            /*_client.ReactionAdded += OnReactionAdded;*/

            _service.CommandExecuted += OnCommandExecuted;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnUserJoined(SocketGuildUser arg)
        {
            var path = await _images.CreateImageAsync(arg);
            await arg.Guild.DefaultChannel.SendFileAsync(path, null);
            File.Delete(path);
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            var prefix = await _servers.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id) ?? _config["prefix"];
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnJoinedGuild(SocketGuild arg)
        {
            if ((arg as IGuild) == null) return;

            var embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithTitle("Hello everyone!")
                .WithDescription("Thanks for inviting me to your server! I hope you have a good time with me!")
                .WithThumbnailUrl(ConstModule.logoUrl)
                .WithFooter(ConstModule.embedFooter)
                .Build();
            await arg.DefaultChannel.SendMessageAsync(embed: embed);
        }
        
        /*private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.MessageId != 776081463170170950) return;
            if (arg3.Emote.Name != "✅") return;

            var role = (arg2 as SocketGuildChannel).Guild.Roles.FirstOrDefault(x => x.Id == 477878753314471936);
            await (arg3.User.Value as SocketGuildUser).AddRoleAsync(role);
        }*/

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await context.Channel.SendMessageAsync($"Error: {result}");
        }

        public async Task LoggedIn()
        {
            await _client.SetStatusAsync(Discord.UserStatus.DoNotDisturb);
            await Activity();
        }

        private async Task Activity()
        {
            int x = 0;
            while (true)
            {
                if (x == 0)
                {
                    await _client.SetGameAsync("/help", null, ActivityType.Listening);
                    x++;
                    Thread.Sleep(10000);
                }
                if (x == 1)
                {
                    await _client.SetGameAsync("https://discord.gg/3stDnz8", null, ActivityType.Playing);
                    x--;
                    Thread.Sleep(10000);
                }
            }
        }
    }
}