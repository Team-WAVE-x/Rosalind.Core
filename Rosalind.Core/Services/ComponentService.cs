using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="dictionaries">버튼의 토큰이 키고 대리자가 값인 딕셔너리</param>
        /// <param name="embed">메시지에 부착할 임베드</param>
        /// <param name="seconds">캐시 시간</param>
        /// <param name="removeMessageAfterTimeOut">캐시 시간 초과시 메시지 삭제 여부</param>
        public void SendComponentMessage(
            string content,
            SocketCommandContext context,
            Dictionary<string, Action> dictionaries,
            Embed embed = null,
            ulong seconds = 500,
            bool removeMessageAfterTimeOut = false)
        {
            var cache = MemoryCache.Default;
            var policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(seconds), RemovedCallback = CacheRemovedCallback };
        }

        private Task OnInteractionCreated(SocketInteraction arg)
        {
            throw new NotImplementedException();
        }

        private void CacheRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            throw new NotImplementedException();
        }
    }
}