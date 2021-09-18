using Discord;
using Discord.Commands;
using Rosalind.Core.Preconditions;
using Rosalind.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.Game
{
    public class Slotmachine : ModuleBase<SocketCommandContext>
    {
        private readonly SqlService _sql;

        public Slotmachine(SqlService sql)
        {
            _sql = sql;
        }

        [Command("슬롯머신")]
        [Cooldown(10)]
        public async Task SlotmachineAsync([Remainder] string coin = null)
        {
            //게임 가능한지 조건 확인
            var user = _sql.GetUser(Context.Guild.Id, Context.User.Id);

            if (string.IsNullOrWhiteSpace(coin))
            {
                await Context.Channel.SendMessageAsync("❌ 배팅할 코인을 입력하여 주세요.");
                return;
            }
            else if (!coin.All(char.IsDigit))
            {
                await Context.Channel.SendMessageAsync("❌ 배팅할 코인은 반드시 소수가 아닌 양수이여야 합니다.");
                return;
            }
            else if (Convert.ToUInt64(coin) < 0 || (Convert.ToDecimal(coin) % 1) > 0)
            {
                await Context.Channel.SendMessageAsync("❌ 배팅할 코인은 반드시 1 이상의 정수여야 합니다.");
                return;
            }
            else if (user.Coin < Convert.ToUInt64(coin))
            {
                await Context.Channel.SendMessageAsync("❌ 코인이 부족합니다.");
                return;
            }

            //게임 로직 시작
            var items = new List<Item>();

            for (int i = 0; i < 3; i++)
            {
                items.Add(RandomEnum());
            }

            var embed = new EmbedBuilder();
            embed.WithTitle("🎲 슬롯머신");
            embed.WithDescription(EnumToEmoji(items[0]).ToString());
            embed.WithColor(Color.Orange);

            var message = await Context.Channel.SendMessageAsync(embed: embed.Build());

            for (int i = 1; i < 3; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                embed.WithDescription(embed.Description + " " + EnumToEmoji(items[i]).ToString());
                await message.ModifyAsync(x => x.Embed = embed.Build());
            }

            var multiply = Multiplier(items);
            var newCoin = Convert.ToUInt64(coin) * multiply;

            if (multiply == 1)
            {
                embed.WithTitle($"💸 꽝..");
                embed.WithColor(Color.Red);
                embed.WithDescription($"슬롯머신에서 `{string.Format("{0:n0}", newCoin)}` 코인을 잃었습니다...");

                _sql.SubUserCoin(Context.Guild.Id, Context.User.Id, newCoin);
            }
            else
            {
                embed.WithTitle($"{EnumToEmoji(items[0])}{EnumToEmoji(items[1])}{EnumToEmoji(items[2])} {multiply}배!");
                embed.WithColor(Color.Green);
                embed.WithDescription($"슬롯머신에서 잭팟이 나와 `{string.Format("{0:n0}", Convert.ToUInt64(newCoin))}` 코인을 얻었습니다!");

                _sql.AddUserCoin(Context.Guild.Id, Context.User.Id, newCoin);
            }

            await message.ModifyAsync(x => x.Embed = embed.Build());
        }

        private enum Item
        {
            Melon,
            Cherry,
            Lemon,
            Star,
            Bell,
            Seven
        }

        private Emoji EnumToEmoji(Item item)
        {
            var items = new Dictionary<Item, Emoji>
            {
                { Item.Melon, new Emoji("\U0001f348") },
                { Item.Cherry, new Emoji("\U0001f352") },
                { Item.Lemon, new Emoji("\U0001f34b") },
                { Item.Star, new Emoji("\u2B50") },
                { Item.Bell, new Emoji("\U0001f514") },
                { Item.Seven, new Emoji("7\u20E3") }
            };

            return items[item];
        }

        private Item RandomEnum()
        {
            Random rd = new Random();
            int value = rd.Next(1, 101);

            if (value <= 30)                         //1 ~ 30
            {
                return Item.Melon;
            }
            else if (value > 30 && value <= 60)      //31 ~ 60
            {
                return Item.Cherry;
            }
            else if (value > 60 && value <= 90)      //61 ~ 90
            {
                return Item.Lemon;
            }
            else if (value > 90 && value <= 95)      //91 ~ 95
            {
                return Item.Star;
            }
            else if (value > 95 && value <= 99)      //96 ~ 99
            {
                return Item.Bell;
            }
            else                                     //100
            {
                return Item.Seven;
            }
        }

        private byte Multiplier(List<Item> emotes)
        {
            if ((emotes[0] == Item.Seven) && (emotes[1] == Item.Seven) && (emotes[2] == Item.Seven))
            {
                return 10;
            }
            else if ((emotes[0] == Item.Star) && (emotes[1] == Item.Star) && (emotes[2] == Item.Star))
            {
                return 7;
            }
            else if ((emotes[0] == Item.Bell) && (emotes[1] == Item.Bell) && (emotes[2] == Item.Bell))
            {
                return 5;
            }
            else if ((emotes[0] == emotes[1]) && (emotes[1] == emotes[2]))
            {
                return 3;
            }
            else if ((emotes[0] == emotes[2]))
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }
    }
}
