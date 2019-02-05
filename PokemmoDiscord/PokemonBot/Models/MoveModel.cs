using System;
using System.Collections.Generic;
using System.Text;

namespace PokemmoDiscord.PokemonBot.Models
{
    class MoveModel
    {
        public float? Accuracy { get; internal set; }
        public string DamageClass { get; internal set; }
        public int ID { get; internal set; }
        public string Name { get; internal set; }
        public string ESPName { get; internal set; }
        public int? Power { get; internal set; }
        public string MovementType { get; internal set; }
    }
}
