using PokeAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemmoDiscord.PokemonBot.Data
{
    class MyPokemon
    {

        public int BaseExperience
        {
            get;
            internal set;
        }

        public bool IsDefault
        {
            get;
            internal set;
        }

        public int Height
        {
            get;
            internal set;
        }
        public int Mass
        {
            get;
            internal set;
        }

        public int Order
        {
            get;
            internal set;
        }

        //se va
        public PokemonAbility[] Abilities
        {
            get;
            internal set;
        }

        public NamedApiResource<PokemonForm>[] Forms
        {
            get;
            internal set;
        }


        public VersionGameIndex[] GameIndices
        {
            get;
            internal set;
        }

        public List<int> Moves
        {
            get;
            internal set;
        }

        public NamedApiResource<PokemonSpecies> Species
        {
            get;
            internal set;
        }

        public PokemonStats[] Stats
        {
            get;
            internal set;
        }

        public PokemonTypeMap[] Types
        {
            get;
            internal set;
        }

        /// <summary>
        /// NOTE: some props can be null, fall back on male, non-shiny (if all shinies are null) values!
        /// </summary>
        public PokemonSprites Sprites
        {
            get;
            internal set;
        }
    }
}
