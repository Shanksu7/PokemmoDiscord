using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemmoDiscord.PokemonBot.Entity
{
    
    public class Trainer
    {
        //Serialized
        [JsonProperty("id")]
        public ulong ID { get; set; }
        [JsonProperty("join")]
        public DateTime Join { get; set; }
        [JsonProperty("cred")]
        public int Credits { get; set; }        
        [JsonProperty("medals")]
        public List<Medal> Medals { get; set; }

        //Not Serialized
        [JsonIgnore]
        public List<Pokemon> Pokemons { get; set; }
    }
}
