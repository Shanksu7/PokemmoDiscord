using Discord;
using Newtonsoft.Json;
using PokemmoDiscord.PokemonBot.Characteristics;
using PokemmoDiscord.PokemonBot.Data;
using PokemmoDiscord.PokemonBot.Mis;
using PokemmoDiscord.PokemonBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemmoDiscord.PokemonBot.Entity
{
    public class PokemonEntity
    {
        public static string File => "pokemon_catched.json";
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
            CurrentExperience = 0;
            Level = x.Next(1, 35 + 1);
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
            RemainingPS = Stats[StatTypeEnum.HP];
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
        public uint CurrentExperience { get; set; }

        [JsonProperty("lvl")]
        public int Level { get; set; }

        [JsonProperty("ps")]
        public int RemainingPS { get; set; }

        [JsonProperty("catched")]
        public DateTime CatchedDay { get; set; }

        [JsonProperty("ivs")]
        StatsCollection IVs { get; set; }

        [JsonProperty("evs")]
        StatsCollection EVs { get; set; }

        [JsonProperty("moves")]
        public List<int> Moves { get; set; }

        [JsonIgnore]
        public List<MoveModel> MovesModels
        {
            get
            {
                return Moves.Select(x => PokemonData.MovesModel.First(y => y.ID == x)).ToList();
            }
        }

        [JsonIgnore]
        public Trainer Trainer
        {
            get
            {
                PokemonData._trainers.TryGetValue(OwnerID, out var e);
                return e;
            }
        }

        [JsonIgnore]
        public int IVPercent
        {
            get
            {
                decimal ivs = IVs.Sum();
                return (int)decimal.Truncate((ivs / (31 * 6)) * 100);
            }
        }

        [JsonIgnore]
        public string Front
        {
            get
            {
                var model = GetModel();
                return model.FrontMale != null ? model.FrontMale : model.FrontFemale;
            }
        }

        [JsonIgnore]
        public string Back
        {
            get
            {
                var model = GetModel();
                return model.BackMale != null ? model.BackMale : model.BackFemale;
            }
        }

        [JsonIgnore]

        public string ShinyFront
        {
            get
            {
                var model = GetModel();
                return model.FrontMaleShiny != null ? model.FrontMaleShiny : model.FrontFemaleShiny;
            }
        }

        [JsonIgnore]
        public string ShinyBack
        {
            get
            {
                var model = GetModel();
                return model.BackMaleShiny != null ? model.BackMaleShiny : model.BackFemaleShiny;
            }
        }

        [JsonIgnore]
        public uint NeededExperience
        {
            get
            {
                return (uint)((4 * Math.Pow(Level, 3)) / 5);
            }
        }
        public bool IsFainted => RemainingPS == 0;

        public PokemonModel GetModel() => PokemonData.PokemonModels.First(x => x.ID == ID);

        public bool HasOwner => OwnerID != 0;

        public void SetOwner(ulong id)
        {
            var t = PokemonData._trainers.GetOrAdd(id, new Trainer(id));
            OwnerID = id;
            CatchedDay = DateTime.Now;
            PokemonData.catchedPokemon.Add(this);
        }

        public Embed EmbedWildInformation()
        {
            var model = GetModel();
            EmbedBuilder eb = new EmbedBuilder()
                .WithImageUrl(model.LargeFront)
                .WithColor(model.GetColor())
                .WithTitle($"**Wild pokemon appeared**")
                .WithDescription("type .catch <name> to get, be fast!");
            return eb.Build();
        }

        public Embed EmbedInformation()
        {

            var model = GetModel();
            EmbedBuilder eb = new EmbedBuilder()
                .WithThumbnailUrl((Shiny) ? ShinyFront : Front)
                .WithColor(model.GetColor())
                .WithTitle($"**#{ID} {Nickname}** ({RemainingPS}/{Stats[StatTypeEnum.HP]})")
                .AddField("IVs:", IVPercent + "%", true)
                .AddField("Lvl" + Level, $" Exp: {CurrentExperience} / {NeededExperience}", true);
            foreach (var stat in Stats.Values)
                eb.AddField(stat.Key.ToString(), stat.Value, true);


            return eb.Build();
        }

        public Embed EmbedMovesInformation()
        {
            var model = GetModel();
            EmbedBuilder eb = new EmbedBuilder()
                .WithThumbnailUrl(GetModel().LargeFront)
                .WithColor(model.GetColor())
                .WithTitle($"**#{ID} {Nickname}**");
            var moves = "";
            foreach (var idm in model.AvailableMoveIDS)
            {
                var move = PokemonData.MovesModel.First(x => x.ID == idm);
                moves += $"[{move.ID}]" + move.ESPName + ", ";
            }
            eb.WithDescription("Available Moves\n\n" + moves);
            string slots = "";
            int count = 1;
            if (MovesModels.Count > 0)
            {
                foreach (var move in MovesModels)
                    slots += $"{count++}) {move.ESPName}\n";
                eb.AddField("Slots", slots);
            }
            else
                eb.AddField("No tienes movimientos aprendidos", "puedes aprender hasta 4");
            return eb.Build();
        }

        public Embed EmbedIVsInformation()
        {

            var model = GetModel();
            EmbedBuilder eb = new EmbedBuilder()
                .WithThumbnailUrl(model.LargeFront)
                .WithColor(model.GetColor())
                .WithTitle($"**#{ID} {Nickname} {IVPercent}%**")
                .WithDescription("**IV's values (max 31)**:");
            foreach (var stat in IVs.Values)
                eb.AddField(stat.Key.ToString(), stat.Value + " / 31", true);
            return eb.Build();
        }

        public bool GiveExperience(PokemonEntity pkmnFainted)
        {
            if (Level < 100)
            {
                uint Total = (uint)((pkmnFainted.HasOwner ? 1.5 : 1) * pkmnFainted.GetModel().BaseExperience * pkmnFainted.Level) / 7;
                CurrentExperience += Total;
                while (CurrentExperience > NeededExperience)
                {
                    CurrentExperience -= NeededExperience;
                    Level++;
                }
                return true;
            }
            else return false;
        }

        public bool DealDamage(int amount)
        {
            RemainingPS -= amount;
            RemainingPS = (RemainingPS < 0) ? 0 : RemainingPS;
            return IsFainted;
        }

        public bool Heal(int amount)
        {
            var ps = 0;
            if (RemainingPS < (ps = Stats[StatTypeEnum.HP]))
            {
                RemainingPS += (ps - RemainingPS);
                return true;
            }
            else
                return false;
        }

        //Call only Stats()
        [JsonIgnore]
        public StatsCollection Stats
        {
            get
            {
                var model = GetModel();
                return new StatsCollection(
                    GetStatValue(StatTypeEnum.SPEED, model.BaseStats[StatTypeEnum.SPEED], IVs[StatTypeEnum.SPEED], 0),
                    GetStatValue(StatTypeEnum.SP_DEF, model.BaseStats[StatTypeEnum.SP_DEF], IVs[StatTypeEnum.SP_DEF], 0),
                    GetStatValue(StatTypeEnum.SP_ATK, model.BaseStats[StatTypeEnum.SP_ATK], IVs[StatTypeEnum.SP_ATK], 0),
                    GetStatValue(StatTypeEnum.DEF, model.BaseStats[StatTypeEnum.DEF], IVs[StatTypeEnum.DEF], 0),
                    GetStatValue(StatTypeEnum.ATK, model.BaseStats[StatTypeEnum.ATK], IVs[StatTypeEnum.ATK], 0),
                    GetStatValue(StatTypeEnum.HP, model.BaseStats[StatTypeEnum.HP], IVs[StatTypeEnum.HP], 0)
                );
            }
        }

        private int GetStatValue(StatTypeEnum type, int _base, int iv, int ev)
        {
            return type switch
            {
                StatTypeEnum.HP => (((2 * _base + iv + (ev / 4)) * Level) / 100) + Level + 10,
                StatTypeEnum.SPEED => (((2 * _base + iv + (ev / 4)) * Level) / 100) + 5,
                _ => (((2 * _base + iv + (ev / 4)) * Level) / 100) + 5
            };
        }

        private int NatureModifier(int value, StatTypeEnum type)
        {
            var increase = 1.1; var decrease = 0.9;
            return Nature switch
            {
                NatureType.LONELY =>
                (type == StatTypeEnum.ATK) ?
                (int)(value * increase) :
                (type == StatTypeEnum.DEF) ?
                (int)(value * decrease) : value,

                NatureType.BRAVE =>
                (type == StatTypeEnum.ATK) ?
                (int)(value * increase) :
                (type == StatTypeEnum.SPEED) ?
                (int)(value * decrease) : value,

                NatureType.ADAMANT =>
                (type == StatTypeEnum.ATK) ?
                (int)(value * increase) :
                (type == StatTypeEnum.SP_ATK) ?
                (int)(value * decrease) : value,

                NatureType.NAUGHTY => (type == StatTypeEnum.ATK) ?
                (int)(value * increase) :
                (type == StatTypeEnum.SP_DEF) ?
                (int)(value * decrease) : value,

                NatureType.BOLD => (type == StatTypeEnum.DEF) ?
                (int)(value * increase) :
                (type == StatTypeEnum.ATK) ?
                (int)(value * decrease) : value,

                NatureType.RELAXED => (type == StatTypeEnum.DEF) ?
                (int)(value * increase) :
                (type == StatTypeEnum.SPEED) ?
                (int)(value * decrease) : value,

                NatureType.IMPISH => (type == StatTypeEnum.DEF) ?
                (int)(value * increase) :
                (type == StatTypeEnum.SP_ATK) ?
                (int)(value * decrease) : value,

                NatureType.LAX => (type == StatTypeEnum.DEF) ?
                (int)(value * increase) :
                (type == StatTypeEnum.SP_DEF) ?
                (int)(value * decrease) : value,

                NatureType.TIMID => (type == StatTypeEnum.SPEED) ?
                (int)(value * increase) :
                (type == StatTypeEnum.ATK) ?
                (int)(value * decrease) : value,

                NatureType.HASTY => (type == StatTypeEnum.SPEED) ?
                (int)(value * increase) :
                (type == StatTypeEnum.DEF) ?
                (int)(value * decrease) : value,

                NatureType.JOLLY => (type == StatTypeEnum.SPEED) ?
                (int)(value * increase) :
                (type == StatTypeEnum.SP_ATK) ?
                (int)(value * decrease) : value,

                NatureType.NAIVE => (type == StatTypeEnum.SPEED) ?
                (int)(value * increase) :
                (type == StatTypeEnum.SP_DEF) ?
                (int)(value * decrease) : value,

                NatureType.MODEST => (type == StatTypeEnum.SP_ATK) ?
                (int)(value * increase) :
                (type == StatTypeEnum.ATK) ?
                (int)(value * decrease) : value,

                NatureType.MILD => (type == StatTypeEnum.SP_ATK) ?
                (int)(value * increase) :
                (type == StatTypeEnum.DEF) ?
                (int)(value * decrease) : value,

                NatureType.QUIET => (type == StatTypeEnum.SP_ATK) ?
                (int)(value * increase) :
                (type == StatTypeEnum.SPEED) ?
                (int)(value * decrease) : value,

                NatureType.RASH => (type == StatTypeEnum.SP_ATK) ?
                (int)(value * increase) :
                (type == StatTypeEnum.SP_DEF) ?
                (int)(value * decrease) : value,

                NatureType.CALM => (type == StatTypeEnum.SP_DEF) ?
                (int)(value * increase) :
                (type == StatTypeEnum.ATK) ?
                (int)(value * decrease) : value,

                NatureType.GENTLE => (type == StatTypeEnum.SP_DEF) ?
                (int)(value * increase) :
                (type == StatTypeEnum.DEF) ?
                (int)(value * decrease) : value,

                NatureType.SASSY => (type == StatTypeEnum.SP_DEF) ?
                (int)(value * increase) :
                (type == StatTypeEnum.SPEED) ?
                (int)(value * decrease) : value,

                NatureType.CAREFUL => (type == StatTypeEnum.SP_DEF) ?
                (int)(value * increase) :
                (type == StatTypeEnum.SP_ATK) ?
                (int)(value * decrease) : value,

                _ => value,
            };
        }

    }
}
