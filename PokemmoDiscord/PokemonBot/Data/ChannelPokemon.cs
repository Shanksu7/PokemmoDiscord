using PokemmoDiscord.PokemonBot.Mis;
using System;
using System.Collections.Generic;

namespace PokemmoDiscord.PokemonBot.Data
{

    public class ChannelPokemon
    {
        public ChannelPokemon(GenerationEnum gen, ChannelEnum chid, int start, int end, List<int> nonspawneable)
        {
            ChId = chid;
            StartIndex = start;
            EndIndex = end;
            NonSpawneablePokemons = nonspawneable;
        }

        public GenerationEnum Generation { get; set; }
        public ChannelEnum ChId { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public List<int> NonSpawneablePokemons { get; set; }

        public int GetRandomPkmnID()
        {
            Random r = new Random(Environment.TickCount.GetHashCode());
            int id = r.Next(StartIndex, EndIndex + 1);
            while (NonSpawneablePokemons.Contains(id))
                id = r.Next(StartIndex, EndIndex + 1);
            return id;
        }
    }
}
