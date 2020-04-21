using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rendy
{
    public class UtilitiesModule : ModuleBase<SocketCommandContext>
    {
        [Command("tell")]
        [RequireOwner]
        public async Task Tell(IUser user, [Remainder] string msg)
        {
            Embed embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Green)
                .WithDescription($"You got a message from Rendy Owner!")
                .WithTitle("TiagoRG")
                .WithFooter($"Rendy{Const.copyright} by TiagoRG#8003")
                .AddField("**Message Content**", msg)
                .AddField("**Sent At**", Context.Message.Timestamp)
                .WithThumbnailUrl(user.GetAvatarUrl())
                .Build();
            await Context.Message.DeleteAsync();
            await user.SendMessageAsync(embed: embed);
            var reply = await Context.Channel.SendMessageAsync("Message sent!");
            await Task.Delay(2000);
            await reply.DeleteAsync();
        }
        [Command("suggest")]
        public async Task Suggest([Remainder] string suggestion)
        {
            await Context.Message.DeleteAsync();
            /*var newChannel = await Context.Guild.CreateTextChannelAsync("suggestions");*/

            var suggestionEmbed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Green)
                .WithDescription("We got a new suggestion for the server!")
                .WithThumbnailUrl(Context.Message.Author.GetAvatarUrl())
                .WithTitle(Context.Message.Author.Username)
                .WithFooter(Const.embedFooter)
                .AddField("Suggestion", suggestion)
                .AddField("Sent At", $"{Context.Message.CreatedAt.Day}/{Context.Message.CreatedAt.Month}/{Context.Message.CreatedAt.Year}")
                .Build();
            /*await newChannel.SendMessageAsync(embed: suggestionEmbed);*/
            await Context.Guild.Owner.SendMessageAsync(embed: suggestionEmbed);
            var reply = await Context.Channel.SendMessageAsync("Your suggestion have been successfully sent!");
            await Task.Delay(1000);
            await reply.DeleteAsync();
        }
    }
}
