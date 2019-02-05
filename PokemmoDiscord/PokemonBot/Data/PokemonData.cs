using AKDiscordBot;
using PokeAPI;
using PokemmoDiscord.PokemonBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PokemmoDiscord.PokemonBot.Data
{
    public class PokemonData
    {
        public static List<PokeAPI.Pokemon> Pokemons = new List<PokeAPI.Pokemon>();
        public static List<string> pokemontypes = new List<string>();
        public static async Task LoadData()
        {
            try
            {
                var x = new Pokemon();

                await LoadMoves();

            }
            catch(Exception ex)
            {
                await Program.SendReport(ex.Message, Program.ReportEnum.ERROR);
            }
            finally
            {

            }
        }
        public static async Task LoadMoves()
        {
            int max = 728;
            for(int i = 1; i <= max; i++)
            {
                var move = await DataFetcher.GetApiObject<Move>(i);
                MoveModel model = new MoveModel()
                {
                    Accuracy = move.Accuracy,
                    DamageClass = move.DamageClass.Name,
                    ID = move.ID,
                    Name = move.Name,
                    ESPName = move.Names[4].Name,
                    Power = move.Power,
                    MovementType = move.Type.Name
                };
            }
        }

    }
}
