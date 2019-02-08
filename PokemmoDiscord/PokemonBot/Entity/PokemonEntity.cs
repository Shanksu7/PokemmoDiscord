using PokemmoDiscord.PokemonBot.Characteristics;
using PokemmoDiscord.PokemonBot.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PokemmoDiscord.PokemonBot.Mis;
using Newtonsoft.Json;
using Discord;
using System.IO;

namespace PokemmoDiscord.PokemonBot.Entity
{
    public class PokemonEntity
    {
        //Serialized
        public PokemonEntity(int id, bool shiny = false, int extraivs = 0, bool maxivs = false)
        {
            Random x = new Random(Environment.TickCount.GetHashCode());
            ID = id;
            var model = GetModel();
            OwnerID = 0;
            Nickname = model.Name;
            Shiny = (shiny) ? true : (x.Next(300 + 1) == 1);
            Nature = (NatureType)(x.Next((int)NatureType.END));
            Experience = 0;
            Lvl = x.Next(1, 35 + 1);
            
            IVs = new StatsCollection                
            (
                maxivs ? 31 : x.Next(31 + 1) + extraivs,
                maxivs ? 31 : x.Next(31 + 1) + extraivs,
                maxivs ? 31 : x.Next(31 + 1) + extraivs,
                maxivs ? 31 : x.Next(31 + 1) + extraivs,
                maxivs ? 31 : x.Next(31 + 1) + extraivs,
                maxivs ? 31 : x.Next(31 + 1) + extraivs
            );
            EVs = new StatsCollection();
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
        [JsonProperty("catched")]
        public DateTime CatchedDay { get; set; }
        [JsonProperty("ivs")]
        StatsCollection IVs { get; set; }
        [JsonProperty("evs")]
        StatsCollection EVs { get; set; }
        [JsonProperty("moves")]
        public List<int> Moves { get; set; }
        public static string File => "pokemon_catched.json";

        public void SetOwner(ulong id)
        {
            var t = PokemonData._trainers.GetOrAdd(id, new Trainer(id));
            OwnerID = id;
            CatchedDay = DateTime.Now;
            PokemonData.catchedPokemon.Add(this);            
        }
        public PokemonModel GetModel() => PokemonData.PokemonModels.First(x => x.ID == ID);
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
            var model = GetModel();
            EmbedBuilder eb = new EmbedBuilder()
                .WithImageUrl(model.GetLargeFront())
                .WithColor(model.GetColor())
                .WithTitle($"**Wild pokemon appeared**")
                .WithDescription("type .catch <name> to get, be fast!");                   
            return eb.Build();
        }
        internal Embed EmbedInformation()
        {

            var model = GetModel();
            EmbedBuilder eb = new EmbedBuilder()
                .WithThumbnailUrl(model.GetLargeFront())
                .WithColor(model.GetColor())
                .WithTitle($"**#{ID} {Nickname}**")
                .AddField("IVs:", GetIVPercent() + "%", true)
                .AddField("Lvl", Lvl, true);                
            foreach (var stat in Stats().Values)
              eb.AddField(stat.Key.ToString(), stat.Value, true);
            
                
            return eb.Build();
        }
        internal Embed EmbedIVsInformation()
        {

            var model = GetModel();
            EmbedBuilder eb = new EmbedBuilder()
                .WithThumbnailUrl(model.GetLargeFront())
                .WithColor(model.GetColor())
                .WithTitle($"**#{ID} {Nickname} {GetIVPercent()}%**")
                .WithDescription("**IV's values (max 31)**:");
            foreach (var stat in IVs.Values)
                eb.AddField(stat.Key.ToString(), stat.Value + " / 31", true);
            return eb.Build();
        }
        public int GetIVPercent()
        {
            decimal ivs = IVs.Sum();
            return (int)decimal.Truncate((ivs / (31 * 6)) * 100);
        }
        

        //Call only Stats()
        public StatsCollection Stats()
        {
            var model = GetModel();
            //have to include variants of nature
            //have to include variants of EV
            int speed = GetStatValue(StatTypeEnum.SPEED, model.BaseStats[StatTypeEnum.SPEED], IVs[StatTypeEnum.SPEED], 0),
                sp_def = GetStatValue(StatTypeEnum.SP_DEF,model.BaseStats[StatTypeEnum.SP_DEF], IVs[StatTypeEnum.SP_DEF],0),
                sp_atk = GetStatValue(StatTypeEnum.SP_ATK, model.BaseStats[StatTypeEnum.SP_ATK], IVs[StatTypeEnum.SP_ATK],0),
                def = GetStatValue(StatTypeEnum.DEF, model.BaseStats[StatTypeEnum.DEF], IVs[StatTypeEnum.DEF],0),
                atk = GetStatValue( StatTypeEnum.ATK,model.BaseStats[StatTypeEnum.ATK], IVs[StatTypeEnum.ATK],0),
                hp = GetStatValue(StatTypeEnum.HP, model.BaseStats[StatTypeEnum.HP], IVs[StatTypeEnum.HP], 0);
            
            return new StatsCollection(            
                speed,
                sp_def,
                sp_atk ,
                def,
                atk,
                hp
            );
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
