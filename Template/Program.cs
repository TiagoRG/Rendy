using System;
using System.IO;
using System.Threading.Tasks;
using Database;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rendy.Services;
using Rendy.Utilities;
using Victoria;

namespace Rendy
{
    class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureDiscordHost<DiscordSocketClient>((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200,
                    };

                    config.Token = context.Configuration["token"];
                })
                .UseCommandService((context, config) =>
                {
                    config = new CommandServiceConfig()
                    {
                        CaseSensitiveCommands = false,
                        LogLevel = LogSeverity.Verbose
                    };
                })
                .ConfigureServices((context, services) =>
                {
                    services
                    .AddHostedService<CommandHandler>()
                    .AddDbContext<RendyContext>()
                    .AddSingleton<InteractiveService>()
                    .AddSingleton<Servers>()
                    .AddSingleton<Images>()
                    .AddSingleton<Mutes>()
                    .AddSingleton<RestoreRoles>()
                    .AddSingleton<Bans>()
                    .AddSingleton<Ranks>()
                    .AddSingleton<AutoRoles>()
                    .AddSingleton<MuteWhitelists>()
                    .AddSingleton<RanksHelper>()
                    .AddSingleton<AutoRolesHelper>()
                    .AddSingleton<MuteWhitelistsHelper>()
                    .AddSingleton<ModSettings>();
                })
                .UseConsoleLifetime();
            
            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}