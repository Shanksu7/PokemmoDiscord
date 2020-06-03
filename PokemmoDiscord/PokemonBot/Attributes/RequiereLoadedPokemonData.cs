using Discord.Commands;
using PokemmoDiscord.PokemonBot.Data;
using System;
using System.Threading.Tasks;
namespace AKDiscordBot.Extensions
{
    class RequireLoadedPokemonAttribute : PreconditionAttribute
    {


        public override Task<PreconditionResult> CheckPermissionsAsync(
            ICommandContext context, CommandInfo command, IServiceProvider services)
            => (PokemonData.Ready) ?
            Task.FromResult(PreconditionResult.FromSuccess()) :
            Task.FromResult(PreconditionResult.FromError("data isnt ready yet"));
    }
}
