using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace discord_bot.Commands
{
    public class SocialCommands : BaseCommandModule
    {
        [Command("github")]
        public async Task Github(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                ImageUrl = "https://pbs.twimg.com/media/E3RmXIwXwAQQgmE?format=jpg&name=large",
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = "Experiências com C# , .NETCore , ASPNET , SQL, AngularJS, React , Typescript" },
                Color = DiscordColor.Purple
            });
            await ctx.Channel.SendMessageAsync($"Salve {ctx.User.Username} espero que goste do meu repositório. \r\n https://github.com/Caio26Gualberto");
        }

        [Command("linkedin")]
        public async Task Linkedin(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = "https://1000logos.net/wp-content/uploads/2023/01/LinkedIn-logo.png" },
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = "Meu Linkedin, espero que goste dos meus posts, e pra você que está começando agora tenho um artigo bem legal!" },
                Color = DiscordColor.Blue,
                Title = "Linkedin",
                Timestamp = DateTimeOffset.Now,
                Author = new DiscordEmbedBuilder.EmbedAuthor() { Name = "Caio Gualberto" }
            });
            await ctx.Channel.SendMessageAsync($"https://www.linkedin.com/in/caio-faria-gualberto-50706019a/");
        }
    }
}
