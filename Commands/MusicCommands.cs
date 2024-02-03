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

        [Command("play")]
        public async Task Join(CommandContext ctx, [RemainingText] string search)
        {
            var lavalink = ctx.Client.GetLavalink();
            var node = lavalink.ConnectedNodes.Values.FirstOrDefault();

            if (node == null)
            {
                await ctx.RespondAsync("Opa, parece que o lavalink está com problemas, o cabaço me programou mal");
            }
            else
            {
                var channel = ctx.Member?.VoiceState?.Channel;

                if (channel != null)
                {
                    var voiceConected = await node.ConnectAsync(channel);
                }
                else
                {
                    await ctx.RespondAsync("Você precisa estar em um canal para o bot saber onde entrar, mas é muito burro mesmo");
                    return;
                }
            }

            node = lavalink.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Opa, parece que o lavalink está com problemas, o cabaço me programou mal");
                return;
            }

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Não achei nada pra essa pesquisa {search}.");
                return;
            }

            var tracks = loadResult.Tracks;

            if (tracks.Count() == 1)
            {
                var track = tracks.First();

                // Adiciona a música à fila
                EnqueueTrack(ctx.Member.VoiceState.Guild.Id, track);

                await ctx.RespondAsync($"Adicionado à fila: {track.Title}");

                // Se não estiver tocando nada, comece a tocar
                if (!IsPlaying(ctx.Member.VoiceState.Guild.Id))
                {
                    await PlayNextTrack(ctx, ctx.Member.VoiceState.Guild.Id);
                }
            }
            else if (tracks.Count() > 1)
            {
                // Se a pesquisa retornar várias músicas, você pode querer permitir ao usuário escolher uma
                // ou simplesmente adicionar todas à fila. Neste exemplo, adicionamos todas à fila.

                foreach (var track in tracks)
                {
                    EnqueueTrack(ctx.Member.VoiceState.Guild.Id, track);
                }

                string queueString = string.Join(", ", _musicQueues
                                                        .Where(pair => pair.Value.Count > 0)
                                                        .SelectMany(pair => pair.Value)
                                                        .Select(track => track.Title));

                await ctx.RespondAsync($"Músicas na fila: {queueString}");

                // Se não estiver tocando nada, comece a tocar
                if (!IsPlaying(ctx.Member.VoiceState.Guild.Id))
                {
                    await PlayNextTrack(ctx, ctx.Member.VoiceState.Guild.Id);
                }
            }
        }

        [Command("skip")]
        public async Task Skip(CommandContext ctx)
        {
            var lavalink = ctx.Client.GetLavalink();
            var node = lavalink.ConnectedNodes.Values.FirstOrDefault();

            if (node == null)
            {
                // O Lavalink não está conectado, maneje isso conforme necessário
                return;
            }

            var guildId = ctx.Member.VoiceState.Guild;

            if (!_musicQueues.TryGetValue(ctx.Member.VoiceState.Guild.Id, out var queue) || queue.Count == 0)
            {
                await ctx.RespondAsync("Não há músicas na fila para pular.");
                return;
            }

            var conn = node.GetGuildConnection(guildId);

            // Para a música atual
            await conn.StopAsync();

            // Pula para a próxima música na fila (se houver)
            await PlayNextTrack(ctx, ctx.Member.VoiceState.Guild.Id);

            await ctx.RespondAsync("Música pulada com sucesso.");
        }


        [Command("stop")]
        public async Task Stop(CommandContext ctx)
        {
            var node = ctx.Client.GetLavalink().ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
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

        private async Task PlayNextTrack(CommandContext ctx, ulong guildId)
        {
            if (_musicQueues.TryGetValue(guildId, out var queue) && queue.Count > 0)
            {
                var nextTrack = queue.Dequeue();
                var node = ctx.Client.GetLavalink().ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                await conn.PlayAsync(nextTrack);

                await ctx.RespondAsync($"Now playing {nextTrack.Title}!");

                conn.PlaybackFinished += async (sender, args) =>
                {
                    // Quando a música termina, toque a próxima da fila (se houver)
                    await PlayNextTrack(ctx, guildId);
                };
            }
        }

    }
}
