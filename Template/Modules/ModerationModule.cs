using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Rendy.Modules
{
    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ModerationModule> _logger;

        public ModerationModule(ILogger<ModerationModule> logger)
        {
            _logger = logger;
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

            await Context.Message.Author.SendMessageAsync($"{messages.Count()} messages got deleted successfully. Enjoy your clean chat!");
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
                .WithFooter(ConstModule.embedFooter)
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
                .WithFooter(ConstModule.embedFooter)
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
                .WithFooter(ConstModule.embedFooter)
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
                .WithFooter(ConstModule.embedFooter)
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
        public async Task SoftBan(IUser user, int days, [Remainder] string reason = null)
        {
            await Context.Message.DeleteAsync();
            Embed embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Red)
                .WithTitle(user.Username + "#" + user.Discriminator)
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithDescription($"This user got softbanned from {Context.Guild.Name} to delete their messages.")
                .WithFooter(ConstModule.embedFooter)
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
                .WithFooter(ConstModule.embedFooter)
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
    }
}
