namespace Rosalind.Core.Models
{
    public class User
    {
        public ulong DatabaseId { get; }
        public ulong GuildId { get; }
        public ulong UserId { get; }
        public ulong Coin { get; }

        public User(ulong id, ulong guildId, ulong userId, ulong coin)
        {
            this.DatabaseId = id;
            this.GuildId = guildId;
            this.UserId = userId;
            this.Coin = coin;
        }
    }
}
