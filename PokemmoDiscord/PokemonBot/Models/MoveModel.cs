using Newtonsoft.Json;
using PokemmoDiscord.PokemonBot.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemmoDiscord.PokemonBot.Models
{
    public class MoveModel
    {
        public static string File = "moves.json";
        [JsonProperty("id")]
        public int ID { get; internal set; }
        [JsonProperty("acc")]
        public float? Accuracy { get; internal set; }
        [JsonProperty("dc")]
        public DamageType DamageClass { get; internal set; }        
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("esp")]
        public string ESPName { get; internal set; }
        [JsonProperty("pw")]
        public int? Power { get; internal set; }
        [JsonProperty("type")]
        public PokemonTypeEnum MovementType { get; internal set; }
        public override string ToString()
        {
            return $"{Name} [{Accuracy},{Power}] {DamageClass.ToString()} {MovementType.ToString()} ";
        }
    }
}
