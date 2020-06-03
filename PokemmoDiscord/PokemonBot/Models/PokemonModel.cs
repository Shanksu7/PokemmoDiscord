using Discord;
using Newtonsoft.Json;
using PokeAPI;
using PokemmoDiscord.PokemonBot.Characteristics;
using PokemmoDiscord.PokemonBot.Mis;
using System.Collections.Generic;
using System.Linq;

namespace PokemmoDiscord.PokemonBot.Data
{
    public class PokemonModel
    {
        public static string File = "pokemon_model.json";
        public string LargeFront => $"https://assets.pokemon.com/assets/cms2/img/pokedex/full/{ID.NumericString()}.png";

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
        public int ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("exp")]
        public int BaseExperience { get; set; }

        [JsonProperty("bm")]
        public string BackMale { get; set; }

        [JsonProperty("bf")]
        public string BackFemale { get; set; }

        [JsonProperty("bms")]
        public string BackMaleShiny { get; set; }

        [JsonProperty("bfs")]
        public string BackFemaleShiny { get; set; }

        [JsonProperty("fm")]
        public string FrontMale { get; set; }

        [JsonProperty("ff")]
        public string FrontFemale { get; set; }

        [JsonProperty("fms")]
        public string FrontMaleShiny { get; set; }

        [JsonProperty("ffs")]
        public string FrontFemaleShiny { get; set; }

        [JsonProperty("types")]
        public List<PokemonTypeEnum> Types { get; set; }

        [JsonProperty("stats")]
        public StatsCollection BaseStats { get; set; }

        [JsonProperty("effort")]
        public StatsCollection EffortStats { get; set; }

        [JsonProperty("moves")]
        public List<int> AvailableMoveIDS { get; set; }

        internal Embed EmbedBaseInformation()
        {
            var eb = new EmbedBuilder()
                .WithThumbnailUrl(LargeFront)
                .WithColor(GetColor())
                .WithTitle($"**#{ID} {Name}**")
                .WithDescription("**Base stats**:");
            foreach (var stat in BaseStats.Values)
                eb.AddField(stat.Key.ToString(), stat.Value, true);
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
            foreach (var stat in stats)
            {
                StatTypeEnum type = StatTypeEnum.ATK;
                switch (stat.Stat.Name)
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
            return Types[0] switch
            {
                PokemonTypeEnum.ACERO => new Color(0xB7B7CE),
                PokemonTypeEnum.AGUA => new Color(0x6390F0),
                PokemonTypeEnum.BICHO => new Color(0xA6B91A),
                PokemonTypeEnum.DRAGON => new Color(0x6F35FC),
                PokemonTypeEnum.ELECTRICO => new Color(0xF7D02C),
                PokemonTypeEnum.FANTASMA => new Color(0x6F35FC),
                PokemonTypeEnum.FUEGO => new Color(0xEE8130),
                PokemonTypeEnum.HADA => new Color(0xD685AD),
                PokemonTypeEnum.HIELO => new Color(0x96D9D6),
                PokemonTypeEnum.HIERBA => new Color(0x7AC74C),
                PokemonTypeEnum.LUCHA => new Color(0xC22E28),
                PokemonTypeEnum.NORMAL => new Color(0xA8A77A),
                PokemonTypeEnum.PSIQUICO => new Color(0xF95587),
                PokemonTypeEnum.ROCA => new Color(0xB6A136),
                PokemonTypeEnum.SINIESTRO => new Color(0x705746),
                PokemonTypeEnum.TIERRA => new Color(0xE2BF65),
                PokemonTypeEnum.VENENO => new Color(0xA33EA1),
                PokemonTypeEnum.VOLADOR => new Color(0xA98FF3),
                _ => new Color(0xFFFFFF),
            };
        }
    }
}
