using Newtonsoft.Json;
using PokeAPI;
using PokemmoDiscord.PokemonBot.Characteristics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PokemmoDiscord.PokemonBot.Mis;
using Discord;

namespace PokemmoDiscord.PokemonBot.Data
{
    public class PokemonModel
    {
        public static string File = "pokemon_model.json";
        public string GetLargeFront() =>  $"https://assets.pokemon.com/assets/cms2/img/pokedex/full/{ID.NumericString()}.png";
        public PokemonModel(Pokemon pokemon, List<int> idmoves)
        {
            ID = pokemon.ID;
            Name = pokemon.Name;
            BaseExperience = pokemon.BaseExperience;
            AvailableMoveIDS = idmoves;
            BackMale = pokemon.Sprites.BackMale;
            BackFemale = pokemon.Sprites.BackFemale;
            BackMaleShiny = pokemon.Sprites.BackShinyMale;
            BackFemaleShiny = pokemon.Sprites.BackFemale;
            FrontMale = pokemon.Sprites.FrontMale;
            FrontFemale = pokemon.Sprites.FrontFemale;
            FrontMaleShiny = pokemon.Sprites.FrontShinyMale;
            FrontFemaleShiny = pokemon.Sprites.FrontShinyFemale;
            Types = pokemon.Types.Select(x => PokemonData._pokemontypes[x.Type.Name]).ToList();
            BaseStats = new StatsCollection();
            EffortStats = new StatsCollection();
        }
        public PokemonModel()
        {

        }
        [JsonProperty("id")]
        public int ID { get;  set; }
        [JsonProperty("name")]
        public string Name { get;  set; }
        [JsonProperty("exp")]
        public int BaseExperience { get;  set; }        
        [JsonProperty("bm")]
        public string BackMale { get;  set; }
        [JsonProperty("bf")]
        public string BackFemale { get;  set; }
        [JsonProperty("bms")]
        public string BackMaleShiny { get;  set; }
        [JsonProperty("bfs")]
        public string BackFemaleShiny { get;  set; }
        [JsonProperty("fm")]
        public string FrontMale { get;  set; }
        [JsonProperty("ff")]
        public string FrontFemale { get;  set; }
        [JsonProperty("fms")]
        public string FrontMaleShiny { get;  set; }
        [JsonProperty("ffs")]
        public string FrontFemaleShiny { get;  set; }
        [JsonProperty("types")]
        public List<PokemonTypeEnum> Types { get;  set; }
        [JsonProperty("stats")]
        public StatsCollection BaseStats { get;  set; }
        [JsonProperty("effort")]
        public StatsCollection EffortStats { get;  set; }
        [JsonProperty("moves")]
        public List<int> AvailableMoveIDS { get;  set; }
        internal Embed EmbedBaseInformation()
        {
            EmbedBuilder eb = new EmbedBuilder()
                .WithThumbnailUrl(GetLargeFront())
                .WithColor(GetColor())
                .WithTitle($"**#{ID} {Name}**")
                .WithDescription("**Base stats**:");
            foreach (var stat in BaseStats.Values)
                eb.AddField(stat.Key.ToString(), stat.Value , true);
            eb.AddField("Base Experience", BaseExperience, true);
            var moves = "";
            foreach (var idm in AvailableMoveIDS)
            {
                var move = PokemonData.MovesModel.First(x => x.ID == idm);
                moves += move.Name + ", ";
            }            
            eb.AddField("Available Moves", moves, true);
            return eb.Build();
        }

        public void BuildIvsEvs(PokemonStats[] stats)
        {
            foreach(var stat in stats)
            {
                StatTypeEnum type = StatTypeEnum.ATK;
                switch(stat.Stat.Name)
                {
                    case "speed":
                        type = StatTypeEnum.SPEED;
                        break;
                    case "special-defense":
                        type = StatTypeEnum.SP_DEF;
                        break;
                    case "special-attack":
                        type = StatTypeEnum.SP_ATK;
                        break;
                    case "defense":
                        type = StatTypeEnum.DEF;
                        break;
                    case "attack":
                        type = StatTypeEnum.ATK;
                        break;
                    case "hp":
                        type = StatTypeEnum.HP;
                        break;
                }
                BaseStats.Add(type, stat.BaseValue);
                EffortStats.Add(type, stat.Effort);
            }
        }

        internal Color GetColor()
        {
            var maintype = Types[0];
            switch (maintype)
            {
                case PokemonTypeEnum.ACERO:
                    return new Color(0xB7B7CE);
                case PokemonTypeEnum.AGUA:
                    return new Color(0x6390F0);
                case PokemonTypeEnum.BICHO:
                    return new Color(0xA6B91A);
                case PokemonTypeEnum.DRAGON:
                    return new Color(0x6F35FC);
                case PokemonTypeEnum.ELECTRICO:
                    return new Color(0xF7D02C);
                case PokemonTypeEnum.FANTASMA:
                    return new Color(0x6F35FC);
                case PokemonTypeEnum.FUEGO:
                    return new Color(0xEE8130);
                case PokemonTypeEnum.HADA:
                    return new Color(0xD685AD);
                case PokemonTypeEnum.HIELO:
                    return new Color(0x96D9D6);
                case PokemonTypeEnum.HIERBA:
                    return new Color(0x7AC74C);
                case PokemonTypeEnum.LUCHA:
                    return new Color(0xC22E28);
                case PokemonTypeEnum.NORMAL:
                    return new Color(0xA8A77A);
                case PokemonTypeEnum.PSIQUICO:
                    return new Color(0xF95587);
                case PokemonTypeEnum.ROCA:
                    return new Color(0xB6A136);
                case PokemonTypeEnum.SINIESTRO:
                    return new Color(0x705746);
                case PokemonTypeEnum.TIERRA:
                    return new Color(0xE2BF65);
                case PokemonTypeEnum.VENENO:
                    return new Color(0xA33EA1);
                case PokemonTypeEnum.VOLADOR:
                    return new Color(0xA98FF3);
                default:
                    return new Color(0xFFFFFF);
            }
        }
    }
}
