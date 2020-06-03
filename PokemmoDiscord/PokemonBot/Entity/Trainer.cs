using AKDiscordBot;
using Discord;
using Newtonsoft.Json;
using PokemmoDiscord.PokemonBot.Data;
using PokemmoDiscord.PokemonBot.Mis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemmoDiscord.PokemonBot.Entity
{

    public class Trainer
    {
        public static string File = "trainers.json";

        public Trainer()
        {


        }
        public Trainer(ulong id)
        {
            ID = id;
            Join = DateTime.Now;
            Credits = 0;
            Redeem = 0;
            Medals = new List<MedalEnum>();
        }

        //Serialized
        [JsonProperty("id")]
        public ulong ID { get; set; }

        [JsonProperty("p_selected")]
        public int SelectedPokemonIndex { get; internal set; }

        [JsonProperty("join")]
        public DateTime Join { get; set; }

        [JsonProperty("cred")]
        public int Credits { get; set; }

        [JsonProperty("redeem")]
        public int Redeem { get; set; }

        [JsonProperty("medals")]
        public List<MedalEnum> Medals { get; set; }

        //Not Serialized
        [JsonIgnore]
        public List<PokemonEntity> Pokemons => PokemonData.catchedPokemon.Where(x => x.OwnerID == ID).OrderBy(x => x.CatchedDay).ToList();
        [JsonIgnore]
        public PokemonEntity SelectedPokemon => Pokemons[SelectedPokemonIndex] ?? Pokemons[0];
        [JsonIgnore]
        public IUser DiscordUser => Program.Client.GetUser(ID);
        [JsonIgnore]
        public bool InFight => (Fight != null ? Fight.State == FightState.RUNNING : false);
        [JsonIgnore]
        public PokemonFight Fight;

        public PokemonEntity SetSelectedPokemon(int ind)
        {
            SelectedPokemonIndex = ind;
            return Pokemons[ind];
        }

        public void GiveCredits(int amount)
        {
            Credits += amount;
            Credits = (Credits < 0) ? 0 : Credits;
        }

    }
}
