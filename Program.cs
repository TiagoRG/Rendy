using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Rendy
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandService commands;
        private IServiceProvider services;
        public enum UserStatus { DoNotDisturb }

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();
        private static void SetConsole()
        {
            Console.Title = "Rendy Console";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Console started!\n\n");
        }
        public async Task MainAsync()
        {
            SetConsole();
            _client = new DiscordSocketClient();

            _client.Log += Log;
            _client.MessageReceived += HandleCommand;
            commands = new CommandService();

            services = new ServiceCollection()
                .AddSingleton(this)
                .AddSingleton(_client)
                .AddSingleton(commands)
                .AddSingleton<ConfigHandler>()
                .BuildServiceProvider();

            await services.GetService<ConfigHandler>().PopulateConfig();

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);

            await _client.LoginAsync(TokenType.Bot, services.GetService<ConfigHandler>().GetToken());
            await _client.StartAsync();

            await _client.SetStatusAsync(Discord.UserStatus.DoNotDisturb);
            await _client.SetGameAsync("/help | discord.gg/3stDnz8", null, ActivityType.Listening);

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.IsBot) return;
            int argPos = 0;
            if (!(message.HasCharPrefix('/', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            var context = new SocketCommandContext(_client, message);
            var result = await commands.ExecuteAsync(context, argPos, services);
            if (context.Message.Author.IsBot) return;
            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
                Console.WriteLine(context.Message.Author + " tried to use a command but it failed. Reason: " + result.ErrorReason);
            }
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
