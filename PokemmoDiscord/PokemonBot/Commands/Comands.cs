using AKDiscordBot.Extensions;
using Discord.Commands;
using PokeAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using PokemmoDiscord.PokemonBot.Attributes;
using PokemmoDiscord.PokemonBot.Data;
using PokemmoDiscord.PokemonBot.Entity;
using Discord;
using PokemmoDiscord.PokemonBot.Mis;

namespace PokemmoDiscord.PokemonBot.Commands
{
    public class Comands : ModuleBase<SocketCommandContext>
    {
        
        [Command("catch", true), RequiereSpawnChannel]
        public async Task Catch(string name)
        {
            if (ChannelPokemon.PokemonInChannel.TryGetValue(Context.Channel.Id, out var wildPokemon))
                if (wildPokemon.Nickname.ToLower() == name.ToLower())
                {
                    ChannelPokemon.PokemonInChannel.Remove(Context.Channel.Id, out var _removedpokemon);
                    PokemonData._trainers.TryGetValue(Context.User.Id, out var trainer);
                    wildPokemon.SetOwner(Context.User.Id);
                    PokemonModel model = wildPokemon.Model;
                    EmbedBuilder eb = new EmbedBuilder();
                    eb.WithTitle($"Congratz {Context.User.Username}! You catched a \n**{model.Name.ToUpper()}** \t [#{model.ID}]");
                    eb.WithThumbnailUrl(model.LargeFront);
                    eb.AddField("Lvl", wildPokemon.Level, true);
                    eb.AddField("IVs", wildPokemon.IVPercent+ "%", true);
                    if (wildPokemon.Shiny)
                        eb.AddField("Wow!", "Shiny pokemon", true);
                    eb.WithColor(model.GetColor());
                    trainer.SelectedPokemon.GiveExperience(wildPokemon);
                    var embed = eb.Build();                    
                    await ReplyAsync(embed: embed);
                }
                else
                    await ReplyAsync("That's not the name of the pokemon try again");
        }
        
    }
}
