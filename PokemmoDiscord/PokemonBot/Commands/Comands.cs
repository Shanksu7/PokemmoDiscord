using AKDiscordBot.Extensions;
using Discord.Commands;
using PokeAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace PokemmoDiscord.PokemonBot.Commands
{
    public class Comands : ModuleBase<SocketCommandContext>
    {
        [Command("roles"), RequireAdminId]
        public async Task Roles()
        {
            var result = "";
            foreach (var role in Context.Guild.Roles)
                result += role.Mention + " " + role.Id + "\n";
            await ReplyAsync(result);
            
        }
        [Command("get"), RequireAdminId]
        public async Task Roles(int id)
        {
            var x = await DataFetcher.GetApiObject<Pokemon>(id);

            var stat = x.Stats.Select(y => y.Stat.Name);
        }
    }
}
