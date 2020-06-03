using Newtonsoft.Json;
using PokemmoDiscord.PokemonBot.Data;
using PokemmoDiscord.PokemonBot.Entity;

namespace PokemmoDiscord.PokemonBot.Models
{
    public class MoveModel
    {
        public static string File = "moves.json";

        [JsonProperty("id")]
        public int ID { get; internal set; }

        [JsonProperty("acc")]
        public float? Accuracy { get; internal set; }

        [JsonProperty("dc")]
        public DamageType DamageClass { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("esp")]
        public string ESPName { get; internal set; }

        [JsonProperty("pw")]
        public int? Power { get; internal set; }

        [JsonProperty("type")]
        public PokemonTypeEnum MovementType { get; internal set; }

        public override string ToString()
        {
            return $"{Name} [{Accuracy},{Power}] {DamageClass.ToString()} {MovementType.ToString()} ";
        }

        public int CalculateDamage(PokemonEntity caster, PokemonEntity target)
        {
            return ((
                (((2 * caster.Level) / 5) + 2)
                * (int)Power
                * (caster.Stats[(DamageClass == DamageType.PHYSICAL ? Mis.StatTypeEnum.ATK : Mis.StatTypeEnum.SP_ATK)]
                / target.Stats[(DamageClass == DamageType.PHYSICAL ? Mis.StatTypeEnum.ATK : Mis.StatTypeEnum.SP_ATK)])
                ) / 50) + 2;
        }
    }
}
