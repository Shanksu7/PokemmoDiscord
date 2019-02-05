using AKDiscordBot;
using Newtonsoft.Json;
using PokeAPI;
using PokemmoDiscord.PokemonBot.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemmoDiscord.PokemonBot.Data
{
    public class PokemonData
    {
        public static List<PokeAPI.Pokemon> Pokemons = new List<PokeAPI.Pokemon>();

        public static ConcurrentDictionary<string, PokemonTypeEnum> _pokemontypes = new ConcurrentDictionary<string, PokemonTypeEnum>();
        public static List<MoveModel> MovesModel = new List<MoveModel>();

        public static async Task LoadData()
        {
            try
            {                
                Console.WriteLine("Start Load Data");
                await InitDictionaries();
                
                using (StreamReader r = new StreamReader("moves.json"))
                    MovesModel = JsonConvert.DeserializeObject<List<MoveModel>>(r.ReadToEnd());
                Console.WriteLine("Loaded Moves");
                await LoadPokemonAPI();
            }
            catch(Exception ex)
            {
                await Program.SendReport(ex.Message, Program.ReportEnum.ERROR);
            }
            finally
            {

            }
        }
        public static async Task InitDictionaries()
        {


            _pokemontypes.TryAdd("normal", PokemonTypeEnum.NORMAL); _pokemontypes.TryAdd("steel", PokemonTypeEnum.ACERO);
            _pokemontypes.TryAdd("water", PokemonTypeEnum.AGUA); _pokemontypes.TryAdd("bug", PokemonTypeEnum.BICHO);
            _pokemontypes.TryAdd("dragon", PokemonTypeEnum.DRAGON); _pokemontypes.TryAdd("electric", PokemonTypeEnum.ELECTRICO);
            _pokemontypes.TryAdd("ghost", PokemonTypeEnum.FANTASMA); _pokemontypes.TryAdd("fire", PokemonTypeEnum.FUEGO);
            _pokemontypes.TryAdd("ice", PokemonTypeEnum.HIELO); _pokemontypes.TryAdd("fighting", PokemonTypeEnum.LUCHA);
            _pokemontypes.TryAdd("grass", PokemonTypeEnum.HIERBA); _pokemontypes.TryAdd("psychic", PokemonTypeEnum.PSIQUICO);
            _pokemontypes.TryAdd("rock", PokemonTypeEnum.ROCA); _pokemontypes.TryAdd("dark", PokemonTypeEnum.SINIESTRO);
            _pokemontypes.TryAdd("ground", PokemonTypeEnum.TIERRA); _pokemontypes.TryAdd("poison", PokemonTypeEnum.VENENO);
            _pokemontypes.TryAdd("flying", PokemonTypeEnum.VOLADOR); _pokemontypes.TryAdd("fairy", PokemonTypeEnum.HADA);

            await Task.CompletedTask;
        }
        public static async Task LoadAPIMoves()
        {
            int max = 728;
            
            List<MoveModel> moves = new List<MoveModel>();
            for (int i = 1; i <= max; i++)
            {
                var move = await DataFetcher.GetApiObject<Move>(i);
                if (move.Power == null || move.Accuracy == null)
                    continue;
                if (move.DamageClass.Name != "special" && move.DamageClass.Name != "physical" )
                    continue;
                _pokemontypes.TryGetValue(move.Type.Name, out var type);
                MoveModel model = new MoveModel()
                {
                    Accuracy = move.Accuracy,
                    DamageClass = (move.DamageClass.Name == "special") ? DamageType.SPECIAL : DamageType.PHYSICAL,
                    ID = move.ID,
                    Name = move.Name,
                    ESPName = move.Names[4].Name,
                    Power = move.Power,
                    MovementType = type
                    
                };
                moves.Add(model);
                if (!_pokemontypes.TryGetValue(move.Type.Name, out var t) && move.Type.Name != "grass")
                {
                    Console.WriteLine(move.Type.Name);
                    var k = Console.ReadKey();
                }
            }
            var s = JsonConvert.SerializeObject(moves, Formatting.Indented);
            File.WriteAllText("moves.json", s);
            Console.WriteLine("Finished moves succes");

        }
        public static async Task LoadPokemonAPI()
        {
            int max = 802;
            for(int i = 1; i<=max; i++)
            {
                var pokemon = await DataFetcher.GetApiObject<Pokemon>(i);
                List<int> idmoves = new List<int>();
                foreach (var x in pokemon.Moves)
                {
                    var m = MovesModel.FirstOrDefault(u => u.Name == x.Move.Name);
                    if (m != null)
                    {
                        Console.WriteLine(m.Name);
                        idmoves.Add(m.ID);
                    }
                    else continue;
                }
                PokemonModel model = new PokemonModel()
                {
                    BaseExperience = pokemon.BaseExperience,
                    MoveIDS = idmoves
                    
                };
            }
        }
    }
}
