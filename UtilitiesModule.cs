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
        public async Task Tell(IUser user, [Remainder] string msg)
        {
            Embed embed = new EmbedBuilder()
                .WithAuthor("Rendy")
                .WithColor(Color.Green)
                .WithDescription($"You got a message from {user.Username}!")
                .WithTitle("New Message!")
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
    }
}
