using PokemmoDiscord.PokemonBot.Characteristics;
using PokemmoDiscord.PokemonBot.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PokemmoDiscord.PokemonBot.Mis;
using Newtonsoft.Json;
using Discord;

namespace PokemmoDiscord.PokemonBot.Entity
{
    public class Pokemon
    {
        //Serialized
        public Pokemon(int id, bool shiny = false, bool maxivs = false)
        {
            Random x = new Random(Environment.TickCount.GetHashCode());
            ID = id;
            var model = GetModel();
            OwnerID = 0;
            Nickname = model.Name;
            Shiny = (shiny) ? true : (x.Next(300 + 1) == 1);
            Nature = (NatureType)(x.Next((int)NatureType.END));
            Experience = 0;
            Lvl = x.Next(35 + 1);
            //must be 6
            IVs = new List<int>()
            {
                maxivs ? 31 : x.Next(31 + 1),
                maxivs ? 31 : x.Next(31 + 1),
                maxivs ? 31 : x.Next(31 + 1),
                maxivs ? 31 : x.Next(31 + 1),
                maxivs ? 31 : x.Next(31 + 1),
                maxivs ? 31 : x.Next(31 + 1),
            };
            EVs = new List<int>();
            Moves = new List<int>();
        }
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("owner")]
        public ulong OwnerID { get; set; }
        [JsonProperty("nick")]
        public string Nickname { get; set; }
        [JsonProperty("shiny")]
        public bool Shiny { get; set; }
        [JsonProperty("nature")]        
        public NatureType Nature { get; set; }
        [JsonProperty("exp")]
        public uint Experience { get; set ; }
        [JsonProperty("lvl")]
        public int Lvl { get; set; }
        [JsonProperty("ivs")]
        public List<int> IVs { get; set; }
        [JsonProperty("evs")]
        public List<int> EVs { get; set; }
        [JsonProperty("moves")]
        public List<int> Moves { get; set; }
        private PokemonModel GetModel()
        {
            return PokemonData.PokemonModel.First(x => x.ID == ID);
        }
        public string GetLargeFront() => $"https://assets.pokemon.com/assets/cms2/img/pokedex/full/{ID.NumericString()}.png";
        public string GetFront()
        {
            var model = GetModel();
            return model.FrontMale != null ? model.FrontMale : model.FrontFemale;
        }
        public string GetBack()
        {
            var model = GetModel();
            return model.BackMale != null ? model.BackMale : model.BackFemale;
        }
        public string GetShinyFront()
        {
            var model = GetModel();
            return model.FrontMaleShiny != null ? model.FrontMaleShiny : model.FrontFemaleShiny;
        }
        public string GetShinyBack()
        {
            var model = GetModel();
            return model.BackMaleShiny != null ? model.BackMaleShiny : model.BackFemaleShiny;
        }
        internal Embed EmbedWildInformation()
        {
            decimal ivs = IVs.Sum();
            decimal s = decimal.Truncate( (ivs / (31 * 6)) * 100);
            var model = GetModel();
            EmbedBuilder eb = new EmbedBuilder()
                .WithImageUrl(GetLargeFront())
                .WithColor(GetColor())
                .WithTitle($"**Un pokemon Salvaje ha aparecido**")
                .AddField("Ivs", s+"%");
            //int count = 0;
            //foreach (var stat in Stats())
              //  eb.AddField(((StatTypeEnum)(count++)).ToString(), stat, true);
            return eb.Build();
        }
        private Color GetColor()
        {
            var maintype = GetModel().Types[0];
            switch(maintype)
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

        //Call only Stats()
        public List<int> Stats()
        {
            var model = GetModel();
            //have to include variants of nature
            //have to include variants of EV
            int speed = GetStatValue(StatTypeEnum.SPEED, model.BaseStats[(int)StatTypeEnum.SPEED], IVs[(int)StatTypeEnum.SPEED], 0),
                sp_def = GetStatValue(StatTypeEnum.SP_DEF,model.BaseStats[(int)StatTypeEnum.SP_DEF], IVs[(int)StatTypeEnum.SP_DEF],0),
                sp_atk = GetStatValue(StatTypeEnum.SP_ATK, model.BaseStats[(int)StatTypeEnum.SP_ATK], IVs[(int)StatTypeEnum.SP_ATK],0),
                def = GetStatValue(StatTypeEnum.DEF, model.BaseStats[(int)StatTypeEnum.DEF], IVs[(int)StatTypeEnum.DEF],0),
                atk = GetStatValue( StatTypeEnum.ATK,model.BaseStats[(int)StatTypeEnum.ATK], IVs[(int)StatTypeEnum.ATK],0),
                hp = GetStatValue(StatTypeEnum.HP, model.BaseStats[(int)StatTypeEnum.HP], IVs[(int)StatTypeEnum.HP], 0);
            
            return new List<int>()
            {
                speed,
                sp_def,
                sp_atk ,
                def,
                atk,
                hp,
            };
        }
        
        private int GetStatValue(StatTypeEnum type, int _base, int iv, int ev)
        {         
            
            int result = 0;
            switch(type)
            {
                case StatTypeEnum.HP:
                    result = (((2 * _base + iv + (ev / 4)) * Lvl) / 100) + Lvl + 10;
                    return result;
                case StatTypeEnum.SPEED:
                    result = (((2 * _base + iv + (ev / 4)) * Lvl) / 100) + 5;
                    return result;
                default:
                    result = (((2 * _base + iv + (ev / 4)) * Lvl) / 100) + 5;
                    break;
            }
            switch(Nature)
            {
                default:
                    break;
            }
            return result;
        }
        private int NatureModifier(int value, StatTypeEnum type)
        {
            var increase = 1.1; var decrease = 0.9;
            switch(Nature)
            {
                case NatureType.LONELY:
                    return
                        (type == StatTypeEnum.ATK) ?
                        (int)(value * increase) :
                        (type == StatTypeEnum.DEF) ?
                        (int)(value * decrease) : value;
                case NatureType.BRAVE:
                    return
                        (type == StatTypeEnum.ATK) ?
                        (int)(value * increase) :
                        (type == StatTypeEnum.SPEED) ?
                        (int)(value * decrease) : value;
                case NatureType.ADAMANT:
                    return
                        (type == StatTypeEnum.ATK) ?
                        (int)(value * increase) :
                        (type == StatTypeEnum.SP_ATK) ?
                        (int)(value * decrease) : value;
                case NatureType.NAUGHTY:
                    return
                        (type == StatTypeEnum.ATK) ?
                        (int)(value * increase) :
                        (type == StatTypeEnum.SP_DEF) ?
                        (int)(value * decrease) : value;
                case NatureType.BOLD:
                    return
                        (type == StatTypeEnum.DEF) ?
                        (int)(value * increase) :
                        (type == StatTypeEnum.ATK) ?
                        (int)(value * decrease) : value;
                case NatureType.RELAXED:
                    return
                        (type == StatTypeEnum.DEF) ?
                        (int)(value * increase) :
                        (type == StatTypeEnum.SPEED) ?
                        (int)(value * decrease) : value;
                case NatureType.IMPISH:
                    return
                        (type == StatTypeEnum.DEF) ?
                        (int)(value * increase) :
                        (type == StatTypeEnum.SP_ATK) ?
                        (int)(value * decrease) : value;
                case NatureType.LAX:
                    return
                        (type == StatTypeEnum.DEF) ?
                        (int)(value * increase) :
                        (type == StatTypeEnum.SP_DEF) ?
                        (int)(value * decrease) : value;
                case NatureType.TIMID:
                    return
                        (type == StatTypeEnum.SPEED) ?
                        (int)(value * increase) :
                        (type == StatTypeEnum.ATK) ?
                        (int)(value * decrease) : value;
                case NatureType.HASTY:
                    return
                        (type == StatTypeEnum.SPEED) ?
                        (int)(value * increase) :
                        (type == StatTypeEnum.DEF) ?
                        (int)(value * decrease) : value;
                case NatureType.JOLLY:
                    return
                        (type == StatTypeEnum.SPEED) ?
                        (int)(value * increase) :
                        (type == StatTypeEnum.SP_ATK) ?
                        (int)(value * decrease) : value;
                case NatureType.NAIVE:
                    return
                        (type == StatTypeEnum.SPEED) ?
                        (int)(value * increase) :
                        (type == StatTypeEnum.SP_DEF) ?
                        (int)(value * decrease) : value;
                case NatureType.MODEST:
                    return
                         (type == StatTypeEnum.SP_ATK) ?
                         (int)(value * increase) :
                         (type == StatTypeEnum.ATK) ?
                         (int)(value * decrease) : value;
                case NatureType.MILD:
                    return
                         (type == StatTypeEnum.SP_ATK) ?
                         (int)(value * increase) :
                         (type == StatTypeEnum.DEF) ?
                         (int)(value * decrease) : value;
                case NatureType.QUIET:
                    return
                         (type == StatTypeEnum.SP_ATK) ?
                         (int)(value * increase) :
                         (type == StatTypeEnum.SPEED) ?
                         (int)(value * decrease) : value;
                case NatureType.RASH:
                    return
                         (type == StatTypeEnum.SP_ATK) ?
                         (int)(value * increase) :
                         (type == StatTypeEnum.SP_DEF) ?
                         (int)(value * decrease) : value;
                case NatureType.CALM:
                    return
                         (type == StatTypeEnum.SP_DEF) ?
                         (int)(value * increase) :
                         (type == StatTypeEnum.ATK) ?
                         (int)(value * decrease) : value;
                case NatureType.GENTLE:
                    return
                         (type == StatTypeEnum.SP_DEF) ?
                         (int)(value * increase) :
                         (type == StatTypeEnum.DEF) ?
                         (int)(value * decrease) : value;
                case NatureType.SASSY:
                    return
                         (type == StatTypeEnum.SP_DEF) ?
                         (int)(value * increase) :
                         (type == StatTypeEnum.SPEED) ?
                         (int)(value * decrease) : value;
                case NatureType.CAREFUL:
                    return
                         (type == StatTypeEnum.SP_DEF) ?
                         (int)(value * increase) :
                         (type == StatTypeEnum.SP_ATK) ?
                         (int)(value * decrease) : value;

                default:
                    return value;
                    
            }
        }

    }
}
