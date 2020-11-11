using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Rendy.Modules
{
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<FunModule> _logger;

        public FunModule(ILogger<FunModule> logger)
        {
            _logger = logger;
        }

        [Command("spam")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Spam(string user, string stringTimes)
        {
            string[] messages = { "Stop spamming", "That's what you get for spamming", "You suck", "Look who is spamming know", "Don't you agree that this is funny" };
            int times = Convert.ToInt32(stringTimes);
            Random random = new Random();
            if (times > 25)
            {
                for (int x = 0; x < 25; x++)
                {
                    await Context.Channel.SendMessageAsync($"{messages[random.Next(5)]}, {user}");
                }
            }
            else
            {
                for (int x = 0; x < times; x++)
                {
                    await Context.Channel.SendMessageAsync($"{messages[random.Next(5)]}, {user}");
                }
            }
        }

        [Command("meme")]
        [Alias("reddit")]
        public async Task Meme(string subreddit = null)
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://reddit.com/r/{subreddit ?? "dankmemes"}/random.json?limit=1");
            if (!result.StartsWith("["))
            {
                await Context.Channel.SendMessageAsync("Couldn't find the subreddit you are looking for.");
                return;
            }
            JArray arr = JArray.Parse(result);
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());
            var embed = new EmbedBuilder()
                .WithAuthor(post["author"].ToString())
                .WithTitle(post["title"].ToString())
                .WithDescription("Meme from " + subreddit ?? "dankmemes")
                .WithImageUrl(post["url"].ToString())
                .WithColor(Color.Orange)
                .WithUrl("https://reddit.com" + post["permalink"].ToString())
                .WithFooter(ConstModule.embedFooter)
                .Build();
            await Context.Channel.SendMessageAsync(embed: embed);
        }
    }
}
