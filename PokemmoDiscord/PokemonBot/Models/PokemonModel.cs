using Newtonsoft.Json;
using PokeAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemmoDiscord.PokemonBot.Data
{
    public class PokemonModel
    {
        [JsonProperty("id")]
        public int ID { get; internal set; }
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("exp")]
        public int BaseExperience { get; internal set; }        
        [JsonProperty("bm")]
        public string BackMale { get; internal set; }
        [JsonProperty("bf")]
        public string BackFemale { get; internal set; }
        [JsonProperty("bms")]
        public string BackMaleShiny { get; internal set; }
        [JsonProperty("bfs")]
        public string BackFemaleShiny { get; internal set; }
        [JsonProperty("fm")]
        public string FronMale { get; internal set; }
        [JsonProperty("ff")]
        public string FrontFemale { get; internal set; }
        [JsonProperty("fms")]
        public string FrontMaleShiny { get; internal set; }
        [JsonProperty("ffs")]
        public string FrontFemaleShiny { get; internal set; }
        [JsonProperty("types")]
        public List<PokemonTypeEnum> Types { get; internal set; }
        [JsonProperty("stats")]
        public List<int> BaseStats { get; internal set; }
        [JsonProperty("effort")]
        public List<int> EffortStats { get; internal set; }
        [JsonProperty("moves")]
        public List<int> MoveIDS { get; internal set; }

    }
}
