using Discord.Commands;
using PokemmoDiscord.PokemonBot.Data;
using PokemmoDiscord.PokemonBot.Manager;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PokemmoDiscord.PokemonBot.Attributes
{
    public class RequiereSpawnChannel : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (ChannelPokemonManager.ChannelsToSpawn.Any(x => (ulong) x.ChId == context.Channel.Id))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("No channel permission"));
        }
    }

    public class RequiereNonSpawnChannel : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (!ChannelPokemonManager.ChannelsToSpawn.Any(x => (ulong) x.ChId == context.Channel.Id))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("No channel permission"));
        }
    }
}
