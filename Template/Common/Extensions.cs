using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Rendy.Modules;

namespace Rendy.Common
{
    public static class Extensions
    {
        public static async Task<IMessage> SendSuccessAsync(this ISocketMessageChannel channel, string title, string description, params EmbedFieldBuilder[] embedFields)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(82, 196, 26))
                .WithTitle(title)
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                        .WithName("Success")
                        .WithIconUrl(ConstModule.GetIconUrl("success"));
                })
                .WithFields(embedFields)
                .WithFooter(ConstModule.footer)
                .Build();
            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }
        public static async Task<IMessage> SendErrorAsync(this ISocketMessageChannel channel, string title, string description, string authorName = "Error", SocketUser user = null)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(245, 34, 45))
                .WithTitle(title)
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                        .WithName(authorName)
                        .WithIconUrl(ConstModule.GetIconUrl("error"));
                })
                .WithFooter(ConstModule.footer)
                .Build();
            if (channel == null)
            {
                var message1 = await user.SendMessageAsync(embed: embed);
                return message1;
            }
            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }
        public static async Task<IMessage> SendWarningAsync(this ISocketMessageChannel channel, string title, string description, SocketUser user = null)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(250, 173, 20))
                .WithTitle(title)
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                        .WithName("Warning")
                        .WithIconUrl(ConstModule.GetIconUrl("warning"));
                })
                .WithFooter(ConstModule.footer)
                .Build();
            if (channel == null)
            {
                var message1 = await user.SendMessageAsync(embed: embed);
                return message1;
            }
            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }
        public static async Task<IMessage> SendInfoAsync(this ISocketMessageChannel channel, string title, string description)
        {
            var embed = new EmbedBuilder()
               .WithColor(new Color(69, 157, 245))
               .WithTitle(title)
               .WithDescription(description)
               .WithAuthor(author =>
               {
                   author
                       .WithName("Info")
                       .WithIconUrl(ConstModule.GetIconUrl("info"));
               })
               .WithFooter(ConstModule.footer)
               .Build();
            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }
    }
}
