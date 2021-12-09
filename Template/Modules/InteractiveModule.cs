using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Rendy.Modules
{
    public class InteractiveModule : InteractiveBase
    {
        [Command("mathqa", RunMode = RunMode.Async)]
        public async Task Test_NextMessageAsync()
        {
            await Context.Message.DeleteAsync();
            var dt = new DataTable();
            string[] ops = { "+", "-", "*" };
            int opi = new Random().Next(0, ops.Length);
            string op = ops[opi];

            if (op == "*")
            {
                int num1 = new Random().Next(1, 11);
                int num2 = new Random().Next(1, 11);
                string expression = num1.ToString() + ops[opi] + num2.ToString();

                var exMsg = await ReplyAndDeleteAsync($"{Context.Message.Author.Mention}, what is ``{expression}``? You have 10 seconds to answer.", timeout: new TimeSpan(0, 0, 10));
                var response = await NextMessageAsync(timeout: new TimeSpan(0, 0, 10));
                if (response != null)
                {
                    try
                    {
                        if (Convert.ToDecimal(response.Content) == Convert.ToDecimal(dt.Compute(expression, null)))
                        {
                            Embed embed = new EmbedBuilder()
                                .WithAuthor("Rendy")
                                .WithColor(Color.Green)
                                .WithTitle("**__Right Answer__**")
                                .WithDescription($"Well done! Requested by {Context.Message.Author.Mention}")
                                .AddField("**Question**", $"__{expression}__", true)
                                .AddField("**Given Answer**", $"__{response}__", true)
                                .AddField("**Right Answer**", $"__{dt.Compute(expression, null)}__", true)
                                .AddField("Response Delay", $"{response.Timestamp.Second - exMsg.Timestamp.Second}s and {Math.Abs(response.Timestamp.Millisecond - exMsg.Timestamp.Millisecond)}ms", true)
                                .WithFooter("© Rendy | Made by TiagoRG#8003")
                                .Build();
                            await ReplyAsync(embed: embed);
                        }
                        else
                        {
                            Embed embed = new EmbedBuilder()
                                .WithAuthor("Rendy")
                                .WithColor(Color.Red)
                                .WithTitle("**__Wrong answer!__**")
                                .WithDescription($"Goodlucky next time! Requested by {Context.Message.Author.Mention}")
                                .AddField("**Question**", $"__{expression}__", true)
                                .AddField("**Given Answer**", $"__{response}__", true)
                                .AddField("**Right Answer**", $"__{dt.Compute(expression, null)}__", true)
                                .AddField("Response Delay", $"{response.Timestamp.Second - exMsg.Timestamp.Second}s and {Math.Abs(response.Timestamp.Millisecond - exMsg.Timestamp.Millisecond)}ms", true)
                                .WithFooter("© Rendy | Made by TiagoRG#8003")
                                .Build();
                            await ReplyAsync(embed: embed);
                        }
                    }
                    catch
                    {
                        await ReplyAndDeleteAsync("Please enter a valid number", timeout: TimeSpan.FromSeconds(5));
                    }
                    finally
                    {
                        await response.DeleteAsync();
                        await exMsg.DeleteAsync();
                    }
                }
                else
                {
                    await ReplyAndDeleteAsync("You did not reply before the timeout, be quicker next time!", timeout: new TimeSpan(0, 0, 10));
                }
            }
            else
            {
                int num1 = new Random().Next(1, 51);
                int num2 = new Random().Next(1, 51);
                string expression = num1.ToString() + ops[opi] + num2.ToString();

                var exMsg = await ReplyAndDeleteAsync($"{Context.Message.Author.Mention}, what is ``{expression}``? You have 10 seconds to answer.", timeout: new TimeSpan(0, 0, 10));
                var response = await NextMessageAsync(timeout: new TimeSpan(0, 0, 10));
                if (response != null)
                {
                    try
                    {
                        if (Convert.ToDecimal(response.Content) == Convert.ToDecimal(dt.Compute(expression, null)))
                        {
                            Embed embed = new EmbedBuilder()
                                .WithAuthor("Rendy")
                                .WithColor(Color.Green)
                                .WithTitle("**__Right Answer__**")
                                .WithDescription($"Well done! Requested by {Context.Message.Author.Mention}")
                                .AddField("**Question**", $"__{expression}__", true)
                                .AddField("**Given Answer**", $"__{response}__", true)
                                .AddField("**Right Answer**", $"__{dt.Compute(expression, null)}__", true)
                                .AddField("Response Delay", $"{response.Timestamp.Second - exMsg.Timestamp.Second}s and {Math.Abs(response.Timestamp.Millisecond - exMsg.Timestamp.Millisecond)}ms", true)
                                .WithFooter("© Rendy | Made by TiagoRG#8003")
                                .Build();
                            await ReplyAsync(embed: embed);
                        }
                        else
                        {
                            Embed embed = new EmbedBuilder()
                                .WithAuthor("Rendy")
                                .WithColor(Color.Red)
                                .WithTitle("**__Wrong answer!__**")
                                .WithDescription($"Goodlucky next time! Requested by {Context.Message.Author.Mention}")
                                .AddField("**Question**", $"__{expression}__", true)
                                .AddField("**Given Answer**", $"__{response}__", true)
                                .AddField("**Right Answer**", $"__{dt.Compute(expression, null)}__", true)
                                .AddField("Response Delay", $"{response.Timestamp.Second - exMsg.Timestamp.Second}s and {Math.Abs(response.Timestamp.Millisecond - exMsg.Timestamp.Millisecond)}ms", true)
                                .WithFooter("© Rendy | Made by TiagoRG#8003")
                                .Build();
                            await ReplyAsync(embed: embed);
                        }
                    }
                    catch
                    {
                        await ReplyAndDeleteAsync("Please enter a valid number", timeout: TimeSpan.FromSeconds(5));
                    }
                    finally
                    {
                        await response.DeleteAsync();
                        await exMsg.DeleteAsync();
                    }
                }
                else
                {
                    await ReplyAndDeleteAsync("You did not reply before the timeout, be quicker next time!", timeout: new TimeSpan(0, 0, 10));
                }
            }

            
        }

        /*[Command("paginator")]
        [RequireOwner]
        public async Task Test_Paginator()
        {
            var pages = new[] { "Page 1", "Page 2", "Page 3", "aaaaaa", "Page 5" };

            PaginatedMessage paginatedMessage = new PaginatedMessage()
            {
                Pages = pages,
                Options = new PaginatedAppearanceOptions()
                {
                    DisplayInformationIcon = false,
                    Timeout = new TimeSpan(0, 1, 0)
                },
                Color = Color.Blue,
                Title = "Help Command"
            };

            await PagedReplyAsync(paginatedMessage);
        }*/
    }
}
