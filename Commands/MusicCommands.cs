using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace discord_bot.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        private readonly Dictionary<ulong, Queue<LavalinkTrack>> _musicQueues = new Dictionary<ulong, Queue<LavalinkTrack>>();
        private CommandContext _commandContext { get; set; }
        private LavalinkExtension _lavalink => _commandContext.Client.GetLavalink();
        private LavalinkNodeConnection _nodeConnection => _lavalink.ConnectedNodes.Values.FirstOrDefault();   

        [Command("play")]
        public async Task Join(CommandContext ctx, [RemainingText] string search)
        {
            _commandContext = ctx;

            if (_nodeConnection == null)
            {
                await ctx.RespondAsync("Opa, parece que o lavalink está com problemas, o cabaço me programou mal");
            }
            else
            {
                var channel = _commandContext.Member?.VoiceState?.Channel;

                if (channel != null)
                {
                    var voiceConected = await _nodeConnection.ConnectAsync(channel);
                }
                else
                {
                    await _commandContext.RespondAsync("Você precisa estar em um canal para o bot saber onde entrar, mas é muito burro mesmo");
                    return;
                }
            }

            var conn = _nodeConnection.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await _commandContext.RespondAsync("Opa, parece que o lavalink está com problemas, o cabaço me programou mal");
                return;
            }

            var loadResult = await _nodeConnection.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await _commandContext.RespondAsync($"Não achei nada pra essa pesquisa {search}.");
                return;
            }

            var tracks = loadResult.Tracks;

            if (tracks.Count() == 1)
            {
                var track = tracks.First();

                // Adiciona a música à fila
                EnqueueTrack(_commandContext.Member.VoiceState.Guild.Id, track);

                await _commandContext.RespondAsync($"Adicionado à fila: {track.Title}");

                // Se não estiver tocando nada, comece a tocar
                if (!IsPlaying(_commandContext.Member.VoiceState.Guild.Id))
                {
                    await PlayNextTrack(_commandContext.Member.VoiceState.Guild.Id);
                }
            }
            else if (tracks.Count() > 1)
            {
                // Se a pesquisa retornar várias músicas, você pode querer permitir ao usuário escolher uma
                // ou simplesmente adicionar todas à fila. Neste exemplo, adicionamos todas à fila.

                foreach (var track in tracks)
                {
                    EnqueueTrack(_commandContext.Member.VoiceState.Guild.Id, track);
                }

                string queueString = string.Join(", ", _musicQueues
                                                        .Where(pair => pair.Value.Count > 0)
                                                        .SelectMany(pair => pair.Value)
                                                        .Select(track => track.Title));

                await _commandContext.RespondAsync($"Músicas na fila: {queueString}");

                // Se não estiver tocando nada, comece a tocar
                if (!IsPlaying(_commandContext.Member.VoiceState.Guild.Id))
                {
                    await PlayNextTrack(_commandContext.Member.VoiceState.Guild.Id);
                }
            }
        }

        [Command("skip")]
        public async Task Skip(CommandContext ctx)
        {
            _commandContext = ctx;

            if (_nodeConnection == null)
            {
                // O Lavalink não está conectado, maneje isso conforme necessário
                return;
            }

            var guild = ctx.Member.VoiceState.Guild;

            if (!_musicQueues.TryGetValue(ctx.Member.VoiceState.Guild.Id, out var queue) || queue.Count == 0)
            {
                await _commandContext.RespondAsync("Não há músicas na fila para pular.");
                return;
            }

            var conn = _nodeConnection.GetGuildConnection(guild);

            // Para a música atual
            await conn.StopAsync();

            // Pula para a próxima música na fila (se houver)
            await PlayNextTrack(ctx.Member.VoiceState.Guild.Id);

            await _commandContext.RespondAsync("Música pulada com sucesso.");
        }


        [Command("stop")]
        public async Task Stop(CommandContext ctx)
        {
            _commandContext = ctx;
            var conn = _nodeConnection.GetGuildConnection(ctx.Member.VoiceState.Guild);
            await conn.DisconnectAsync();
        }

        private void EnqueueTrack(ulong guildId, LavalinkTrack track)
        {
            if (!_musicQueues.TryGetValue(guildId, out var queue))
            {
                queue = new Queue<LavalinkTrack>();
                queue.Enqueue(track);
                _musicQueues.Add(guildId, queue);
            }
            else
                queue.Enqueue(track);
        }

        private bool IsPlaying(ulong guildId)
        {
            return !_musicQueues.TryGetValue(guildId, out var queue) && queue.Count > 0;
        }

        private async Task PlayNextTrack(ulong guildId)
        {
            if (_musicQueues.TryGetValue(guildId, out var queue) && queue.Count > 0)
            {
                var nextTrack = queue.Dequeue();
                var conn = _nodeConnection.GetGuildConnection(_commandContext.Member.VoiceState.Guild);

                await conn.PlayAsync(nextTrack);

                await _commandContext.RespondAsync($"Now playing {nextTrack.Title}!");

                conn.PlaybackFinished += async (sender, args) =>
                {
                    // Quando a música termina, toque a próxima da fila (se houver)
                    await PlayNextTrack(guildId);
                };
            }
        }

    }
}
