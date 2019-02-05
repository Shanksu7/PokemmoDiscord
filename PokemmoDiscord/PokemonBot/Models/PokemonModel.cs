using PokeAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemmoDiscord.PokemonBot.Data
{
    class PokemonModel
    {
        public int BaseExperience { get; internal set; }
        public List<int> MoveIDS { get; internal set; }
    }
}
