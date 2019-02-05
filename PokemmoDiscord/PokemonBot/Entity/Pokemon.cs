using PokemmoDiscord.PokemonBot.Characteristics;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemmoDiscord.PokemonBot.Entity
{
    public class Pokemon
    {
        //Serialized
        public int ID { get; set; }
        public ulong OwnerID { get; set; }
        public string Nickname { get; set; }
        public List<int> IVs { get; set; }
        public List<int> EVs { get; set; }
        public NatureType Nature { get; set; }
        public int Experience { get; set ; }
        public int Lvl { get; set; }


        //Not Serialized

    }
}
