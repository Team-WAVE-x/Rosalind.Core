using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rosalind.Core.Models
{
    public class ComponentMessage
    {
        public ulong MessageId { get; }
        public ulong MessageUserId { get; }
        public ulong MessageChannelId { get; }
        public ulong MessageGuildId { get; }
        public Dictionary<Button, Action<SocketInteraction, ComponentMessage>> Dictionary { get; }
        public bool RemoveMessageAfterTimeOut { get; }

        public ComponentMessage(ulong messageId, ulong messageUserId, ulong messageChannelId, ulong messageGuildId, Dictionary<Button, Action<SocketInteraction, ComponentMessage>> dictionary, bool removeMessageAfterTimeOut)
        {
            this.MessageId = messageId;
            this.MessageUserId = messageUserId;
            this.MessageChannelId = messageChannelId;
            this.MessageGuildId = messageGuildId;
            this.Dictionary = dictionary;
            this.RemoveMessageAfterTimeOut = removeMessageAfterTimeOut;
        }
    }
}