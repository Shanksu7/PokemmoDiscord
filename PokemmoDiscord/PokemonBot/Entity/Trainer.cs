using Newtonsoft.Json;
using System.Linq;
using PokemmoDiscord.PokemonBot.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemmoDiscord.PokemonBot.Entity
{
    
    public class Trainer
    {
        public Trainer()
        {
            
            
        }
        public Trainer(ulong id)
        {
            ID = id;
            Join = DateTime.Now;
            Credits = 0;
            Redeem = 0;
            Medals = new List<Medal>();
        }
        public static string File = "trainers.json";
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
        public List<Medal> Medals { get; set; }
        //Not Serialized

        public List<PokemonEntity> GetPokemons() => PokemonData.catchedPokemon.Where(x => x.OwnerID == ID).OrderBy(x => x.CatchedDay).ToList();
        public PokemonEntity GetSelectedPokemon() => GetPokemons()[SelectedPokemonIndex] ?? GetPokemons()[0];

        public PokemonEntity SetSelectedPokemon(int ind)
        {
            SelectedPokemonIndex = ind;
            return GetPokemons()[ind];
        }
        
    }
}
