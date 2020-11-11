using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Rendy.Modules
{
    public class AdministrationModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<AdministrationModule> _logger;
        private readonly Servers _servers;
        private readonly IConfiguration _config;

        public AdministrationModule(ILogger<AdministrationModule> logger, Servers servers, IConfiguration config)
        {
            _logger = logger;
            _servers = servers;
            _config = config;
        }

        [Command("tell")]
        [RequireOwner]
        public async Task Tell(IUser user, [Remainder] string msg)
        {
            Embed embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.DarkerGrey)
                .WithDescription($"You got a message from Rendy Owner!")
                .WithTitle("TiagoRG")
                .WithFooter($"Rendy{ConstModule.copyright} by TiagoRG#8003")
                .AddField("**Message Content**", msg)
                .AddField("**Sent At**", Context.Message.Timestamp)
                .WithThumbnailUrl(user.GetAvatarUrl())
                .Build();
            await Context.Message.DeleteAsync();
            await user.SendMessageAsync(embed: embed);
            await Context.Channel.SendMessageAsync("Message sent!");
            await Context.Channel.SendMessageAsync(embed: embed);
        }

        [Command("prune")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task Prune(int daysoff = 14)
        {
            await Context.Message.DeleteAsync();
            var totalKicks = await Context.Guild.PruneUsersAsync(daysoff);
            var reply = await Context.Channel.SendMessageAsync($"I kicked {totalKicks} members for being offline for more than {daysoff} days.");
            await Task.Delay(5000);
            await reply.DeleteAsync();
        }

        [Command("prefix")]
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
                    .WithColor(Color.DarkerGrey)
                    .WithThumbnailUrl(ConstModule.logoUrl)
                    .WithFooter(ConstModule.embedFooter)
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
                    .WithColor(Color.DarkerGrey)
                    .WithThumbnailUrl(ConstModule.logoUrl)
                    .WithFooter(ConstModule.embedFooter)
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
                    .WithColor(Color.DarkerGrey)
                    .WithThumbnailUrl(ConstModule.logoUrl)
                    .WithFooter(ConstModule.embedFooter)
                    .Build();
            await Context.Channel.SendMessageAsync(embed: embed2);
        }
    }
}
