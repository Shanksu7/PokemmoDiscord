using Newtonsoft.Json;
using System.Linq;
using PokemmoDiscord.PokemonBot.Mis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PokemmoDiscord.PokemonBot.Characteristics
{
    public class StatsCollection
    {
        public StatsCollection(int spd = 0, int atk = 0, int def = 0, int spatk = 0, int spdef = 0, int hp = 0)
        {
            Values = new ConcurrentDictionary<StatTypeEnum, int>();
            Values.TryAdd(StatTypeEnum.SPEED, spd);
            Values.TryAdd(StatTypeEnum.ATK, atk);
            Values.TryAdd(StatTypeEnum.DEF, def);
            Values.TryAdd(StatTypeEnum.SP_ATK, spatk);
            Values.TryAdd(StatTypeEnum.SP_DEF, spdef);
            Values.TryAdd(StatTypeEnum.HP, hp);
        }
        public int this[StatTypeEnum type] => Values[type];
        [JsonProperty("values")]
        public ConcurrentDictionary<StatTypeEnum, int> Values  { get; internal set; }
        public decimal Sum()
        {
            return Values.Values.Sum();
        }
        public void Add(StatTypeEnum type, int value ) => Values.AddOrUpdate(type, value, (key, v) => v = value);
        
    }
}
