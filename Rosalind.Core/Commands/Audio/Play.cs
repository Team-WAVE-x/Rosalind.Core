using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Victoria;

namespace Rosalind.Core.Commands.Audio
{
    public class Play : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Play(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }

        [Command("접속")]
        public async Task PlayAsync()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("❌ 이미 음성 채널에 연결되어 있습니다!");
                return;
            }
            
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("❌ 음성 채널에 연결해 있어야 합니다!");
                return;
            }

            await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            await ReplyAsync($"`#{voiceState.VoiceChannel.Name}`에 접속했습니다!");
        }
    }
}