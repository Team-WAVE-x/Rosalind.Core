using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Rosalind.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Rosalind.Core.Services
{
    public class ComponentService
    {
        private IServiceProvider _service;
        private DiscordSocketClient _client;

        public ComponentService(IServiceProvider service)
        {
            _service = service;
            _client = service.GetRequiredService<DiscordSocketClient>();
            _client.InteractionCreated += OnInteractionCreated;
        }

        /// <summary>
        /// 컴포넌트 메시지를 전송하고 캐싱합니다.
        /// </summary>
        /// <param name="content">메시지의 내용</param>
        /// <param name="context">커맨드의 컨텍스트</param>
        /// <param name="dictionary">버튼 객체가 키고 대리자가 값인 딕셔너리</param>
        /// <param name="embed">메시지에 부착할 임베드</param>
        /// <param name="seconds">캐시 시간</param>
        /// <param name="removeMessageAfterTimeOut">캐시 시간 초과시 메시지 삭제 여부</param>
        public async Task SendComponentMessage(string content, SocketCommandContext context, Dictionary<Button, Action> dictionary, Embed embed = null, ulong seconds = 500, bool removeMessageAfterTimeOut = false)
        {
            var cache = MemoryCache.Default;
            var policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(seconds), RemovedCallback = CacheRemovedCallback };
            var builder = new ComponentBuilder();

            //딕셔너리 키에서 객체를 가져와 버튼으로 하나하나 등록함
            foreach (var item in dictionary)
            {
                builder.WithButton(item.Key.Label, item.Key.CustomId, item.Key.Style, item.Key.Emote, row: item.Key.Row);
            }

            var message = await context.Channel.SendMessageAsync(content, embed: embed, component: builder.Build()); //메시지 전송한 다음 객체 캡쳐
            var componentMessage = new ComponentMessage(message.Id, context.User.Id, context.Channel.Id, context.Guild.Id, dictionary, removeMessageAfterTimeOut);

            cache.Add(message.Id.ToString(), componentMessage, policy); //캐시에 등록

            Console.WriteLine("{0} {1,-11} {2}", DateTime.Now.ToString("HH:mm:ss"), "Component", $"Cached {message.Id}"); //로그 메시지 전송
        }

        private Task OnInteractionCreated(SocketInteraction arg)
        {
            var cache = MemoryCache.Default;
            var socketMessageComponent = arg as SocketMessageComponent;
            var componentMessage = cache.Get(socketMessageComponent.Message.Id.ToString()) as ComponentMessage;

            //가져온 객체 내에 있는 딕셔너리의 대리자를 실행
            foreach (var item in componentMessage.Dictionary)
            {
                if (socketMessageComponent != null && componentMessage != null && socketMessageComponent.Data.CustomId == item.Key.CustomId && socketMessageComponent.User.Id == componentMessage.MessageUserId) //socketMessageComponent과 componentMessage이 null이 아니고 이벤트를 받은 컴포넌트의 아이디와 캐시에서 가져온 컴포넌트의 아이디와 같고 인터랙션을 보낸 사람의 아이디와 캐시에서 가져온 유저의 아이디와 같다면
                {
                    item.Value(); //대리자 실행
                }
            }

            return Task.CompletedTask; //작업 완료
        }

        private async void CacheRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            var componentMessage = arguments.CacheItem.Value as ComponentMessage;

            if (componentMessage.RemoveMessageAfterTimeOut)
            {
                try
                {
                    var message = await (_client.GetGuild(componentMessage.MessageGuildId).GetChannel(componentMessage.MessageChannelId) as ISocketMessageChannel).GetMessageAsync(componentMessage.MessageId) as RestUserMessage;
                    await message.DeleteAsync();
                }
                catch (Exception)
                {
                    Console.WriteLine("{0} {1,-11} {2}", DateTime.Now.ToString("HH:mm:ss"), "Component", $"Error deleting message! ({componentMessage.MessageId})");
                }
            }
        }
    }
}