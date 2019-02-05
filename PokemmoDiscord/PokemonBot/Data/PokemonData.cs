using AKDiscordBot;
using Newtonsoft.Json;
using PokeAPI;
using PokemmoDiscord.PokemonBot.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemmoDiscord.PokemonBot.Data
{
    public class PokemonData
    {
        
        public static ConcurrentDictionary<string, PokemonTypeEnum> _pokemontypes = new ConcurrentDictionary<string, PokemonTypeEnum>();
        public static List<MoveModel> MovesModel = new List<MoveModel>();
        public static List<PokemonModel> PokemonModel = new List<PokemonModel>();
        public static bool Ready = false;
        public static async Task LoadData()
        {
            try
            {
                var sp = new Stopwatch();
                sp.Start();
                Console.WriteLine("Start Load Data");
                //dictionaries
                await InitDictionaries();
                await Program.SetGameAsync("Loading moves");
                using (StreamReader r = new StreamReader("moves.json"))
                    MovesModel = JsonConvert.DeserializeObject<List<MoveModel>>(r.ReadToEnd());
                Console.WriteLine("Loaded "+MovesModel.Count()+" Moves");
                await Program.SetGameAsync("Loading Pokemon models");
                using (StreamReader r = new StreamReader("pokemon_model.json"))
                    PokemonModel = JsonConvert.DeserializeObject<List<PokemonModel>>(r.ReadToEnd());                
                Console.WriteLine("Loaded "+PokemonModel.Count()+" pokemon models");
                sp.Stop();
                Console.WriteLine("Finished after " + sp.ElapsedMilliseconds + " ms");
                await Program.SetGameAsync("guiding " + Program.pre);
                Ready = true;
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

        }
        public static async Task LoadPokemonAPI()
        {
            int max = 802;
            List<PokemonModel> models = new List<PokemonModel>();
            for(int i = 1; i<=max; i++)
            {
                var pokemon = await DataFetcher.GetApiObject<Pokemon>(i);
                List<int> idmoves = new List<int>();
                foreach (var x in pokemon.Moves)
                {
                    var m = MovesModel.FirstOrDefault(u => u.Name == x.Move.Name);
                    if (m != null)
                    {
                        //Console.WriteLine(m.Name);
                        idmoves.Add(m.ID);
                    }
                    else continue;
                }
                if(pokemon.Forms.Count() > 1)                
                    Console.WriteLine(pokemon.Forms.Count() +" for " + pokemon.Name);
                PokemonModel model = new PokemonModel()
                {
                    ID = pokemon.ID,
                    Name = pokemon.Name,
                    BaseExperience = pokemon.BaseExperience,
                    MoveIDS = idmoves,
                    BackMale = pokemon.Sprites.BackMale,
                    BackFemale = pokemon.Sprites.BackFemale ,
                    BackMaleShiny = pokemon.Sprites.BackShinyMale,
                    BackFemaleShiny = pokemon.Sprites.BackFemale,
                    FronMale = pokemon.Sprites.FrontMale,
                    FrontFemale = pokemon.Sprites.FrontFemale,
                    FrontMaleShiny = pokemon.Sprites.FrontShinyMale,
                    FrontFemaleShiny = pokemon.Sprites.FrontShinyFemale,
                    Types = pokemon.Types.Select(x => _pokemontypes[x.Type.Name]).ToList(),
                    BaseStats = pokemon.Stats.Select(x => x.BaseValue).ToList(),
                    EffortStats = pokemon.Stats.Select(x => x.Effort).ToList()
                };
                models.Add(model);
                Console.WriteLine("["+model.ID+"] " + model.Name);
            }
            var s = JsonConvert.SerializeObject(models, Formatting.Indented);
            File.WriteAllText("pokemon_model.json", s);
        }
        public static async Task<List<int>> GetEvolutionIDs(Pokemon poke)
        {
            List<int> ids = new List<int>();
            EvolutionChain evs;
            try
            {
                evs = await DataFetcher.GetApiObject<EvolutionChain>(poke.ID);
            }
            catch(Exception ex) { return ids; }
            
            if(evs.Chain.Details.Count() > 0)
            {
                ChainLink[] l;
              /* ids.Add(_pokemons.First(x => x.Name == evs.Chain.EvolvesTo[0].Species.Name).ID);
                if(( l = evs.Chain.EvolvesTo[0].EvolvesTo).Count() > 0)                
                    ids.AddRange(await GetEvolutionIDs(_pokemons.First(x => x.Name == l[0].Species.Name)));  */              
            }
            return ids;
        }
    }
}
