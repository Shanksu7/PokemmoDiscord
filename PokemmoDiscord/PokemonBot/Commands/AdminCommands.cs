using AKDiscordBot.Extensions;
using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace PokemmoDiscord.PokemonBot.Commands
{
    [RequireAdminId]
    public class AdminCommands : Comands
    {
        [Command("roles")]
        public async Task Roles()
        {
            var result = "";
            foreach (var role in Context.Guild.Roles)
                result += role.Mention + " " + role.Id + "\n";
            await ReplyAsync(result);

        }

        [Command("test", true)]
        public async Task Test()
        {

            EmbedBuilder eb = new EmbedBuilder();
            try
            {

                eb.WithTitle($"Congratz!");
                eb.WithImageUrl("http://i.imgur.com/pGCM1mZ.jpg");
                eb.WithDescription("lol arceus");
                eb.WithThumbnailUrl("https://sites.google.com/site/aurakingdomguia/_/rsrc/1479963432606/trabajos/recoleccion/ingredientes/ingpolvoamarillo.jpg");
                eb.WithColor(Color.Red);
                var embed = eb.Build();
                await ReplyAsync(embed: embed).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                eb.WithTitle($"Congratz! You catched a bug in your code :)");
                eb.WithDescription(ex.Message);
                var embed = eb.Build();
                await ReplyAsync(embed: embed).ConfigureAwait(false);
            }
        }
    }
}
