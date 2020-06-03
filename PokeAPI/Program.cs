using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokeAPI
{
    class Program
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
            
            public PokemonHeldItem[] HeldItems
            {
                get;
                internal set;
            }

            public PokemonMove[] Moves
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
        static void Main(string[] args)
        {
            int opc = 1;
            var p = DataFetcher.GetApiObject<Pokemon>(opc++).GetAwaiter().GetResult();
        }


        class MyPokemon2
        {
            public int ID { get; set; }
            public string Name { get; set; }
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

            public List<string> Moves
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
}
