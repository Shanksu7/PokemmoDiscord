using Discord;
using Discord.Commands;
using PokemmoDiscord.PokemonBot.Attributes;
using PokemmoDiscord.PokemonBot.Data;
using PokemmoDiscord.PokemonBot.Manager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemmoDiscord.PokemonBot.Commands
{
    public class Comands : ModuleBase<SocketCommandContext>
    {

        [Command("catch", true)]
        [RequiereSpawnChannel]
        public async Task Catch(params string[] name)
        {
            if (ChannelPokemonManager.PokemonInChannel.TryGetValue(Context.Channel.Id, out var wildPokemon))
                if (wildPokemon.Nickname.ToLower() == string.Join(' ', name).ToLower())
                {
                    ChannelPokemonManager.PokemonInChannel.Remove(Context.Channel.Id, out var _removedpokemon);
                    PokemonData._trainers.TryGetValue(Context.User.Id, out var trainer);
                    wildPokemon.SetOwner(Context.User.Id);
                    var model = wildPokemon.GetModel();
                    var eb = new EmbedBuilder();
                    eb.WithTitle($"Congratz {Context.User.Username}! You catched a \n**{model.Name.ToUpper()}** \t [#{model.ID}]");
                    eb.WithThumbnailUrl(model.LargeFront);
                    eb.AddField("Lvl", wildPokemon.Level, true);
                    eb.AddField("IVs", wildPokemon.IVPercent + "%", true);
                    if (wildPokemon.Shiny)
                        eb.AddField("Wow!", "Shiny pokemon", true);
                    eb.WithColor(model.GetColor());
                    trainer.SelectedPokemon.GiveExperience(wildPokemon);
                    var embed = eb.Build();
                    await ReplyAsync(embed: embed).ConfigureAwait(false);
                }
                else
                    await ReplyAsync("Wrong pokemon's name try again").ConfigureAwait(false);
        }

    }
}
