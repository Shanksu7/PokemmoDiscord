using Discord.Commands;
using PokemmoDiscord.PokemonBot.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace PokemmoDiscord.PokemonBot.Attributes
{
    class RequiereSpawnChannel : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (ChannelPokemon.ChannelsToSpawn.Any(x => x.ChId == context.Channel.Id))                
                return Task.FromResult(PreconditionResult.FromSuccess());
            else                
                return Task.FromResult(PreconditionResult.FromError("No channel permission"));
            
            
        }
    }
}
