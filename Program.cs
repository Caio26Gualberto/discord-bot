using discord_bot.Commands;
using discord_bot.Configuration;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace discord_bot
{
    internal class Program
    {
        private static DiscordClient _client { get; set; }
        private static CommandsNextExtension _commands { get; set; }
        static async Task Main(string[] args)
        {
            ConfigReader configs = new ConfigReader();
            await configs.ReadJSON();

            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = configs.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            var endpoint = new ConnectionEndpoint
            {
                Hostname = "lavalink.teramont.net",
                Port = 25565,
                Secured = false
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "eHKuFcz67k4lBS64",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };


            _client = new DiscordClient(discordConfig);

            _client.Ready += Ready;
            var lavalink = _client.UseLavalink();

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes =  new string[] { configs.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
                CaseSensitive = false
            };

            _commands = _client.UseCommandsNext(commandsConfig);

            _commands.RegisterCommands<SocialCommands>();
            _commands.RegisterCommands<MusicCommands>();

            await _client.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);
            await Task.Delay(-1);
        }

        private static Task Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
