using Discord.Commands;
using PokemmoDiscord.PokemonBot.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

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
    class RequiereNonSpawnChannel : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (!ChannelPokemon.ChannelsToSpawn.Any(x => x.ChId == context.Channel.Id))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("No channel permission"));
        }
    }
}
