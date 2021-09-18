using Discord;
using Discord.Commands;
using Rosalind.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.General
{
    public class Ping : ModuleBase<SocketCommandContext>
    {
        private readonly ReactService _react;

        public Ping(ReactService react)
        {
            _react = react;
        }

        [Command("핑")]
        public async Task PingAsync()
        {
            var message = await Context.Channel.SendMessageAsync($"Pinging...");
            var latency = message.Timestamp - Context.Message.Timestamp;
            var pingColor = new Color();

            if (latency.TotalMilliseconds < 50)
                pingColor = Color.Green;
            else if (latency.TotalMilliseconds < 150)
                pingColor = Color.LightOrange;
            else if (latency.TotalMilliseconds < 300)
                pingColor = Color.Orange;
            else
                pingColor = Color.Red;

            var embed = new EmbedBuilder();
            embed.WithTitle("🏓 Pong!");
            embed.WithColor(pingColor);
            embed.WithFields(new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder{ Name = "Gateway Ping", Value = $"`{Context.Client.Latency}ms`" },
                    new EmbedFieldBuilder{ Name = "Client Ping", Value = $"`{latency.TotalMilliseconds}ms`" }
                });
            embed.WithFooter(new EmbedFooterBuilder
            {
                IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                Text = Context.User.Username
            });
            embed.WithTimestamp(DateTimeOffset.Now);

            await message.ModifyAsync(msg =>
            {
                msg.Content = null;
                msg.Embed = embed.Build();
            });
        }
    }
}
