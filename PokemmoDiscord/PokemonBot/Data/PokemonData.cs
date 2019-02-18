using AKDiscordBot;
using Newtonsoft.Json;
using PokeAPI;
using PokemmoDiscord.PokemonBot.Entity;
using PokemmoDiscord.PokemonBot.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PokemmoDiscord.PokemonBot.Data
{
    public class PokemonData
    {

        public static ConcurrentDictionary<string, PokemonTypeEnum> _pokemontypes = new ConcurrentDictionary<string, PokemonTypeEnum>();
        public static ConcurrentDictionary<ulong, Trainer> _trainers = new ConcurrentDictionary<ulong, Trainer>();
        public static ConcurrentBag<Entity.PokemonEntity> catchedPokemon = new ConcurrentBag<Entity.PokemonEntity>();
        public static ConcurrentBag<MoveModel> MovesModel = new ConcurrentBag<MoveModel>();
        public static ConcurrentBag<PokemonModel> PokemonModels = new ConcurrentBag<PokemonModel>();
        public static bool Ready = false;
        public static Timer saveData;
        public static int MaxPokemonId = 802;
        public static async Task LoadData()
        {
            try
            {
                var sp = new Stopwatch();
                sp.Start();
                Console.WriteLine("Start Load Data");
                //dictionaries
                await InitDictionaries();
                //MOVES MODELS
                await Program.SetGameAsync("Loading moves");
                using (StreamReader r = new StreamReader(MoveModel.File))
                    MovesModel = JsonConvert.DeserializeObject<ConcurrentBag<MoveModel>>(r.ReadToEnd());
                Console.WriteLine("Loaded " + MovesModel.Count() + " Moves\n");
                //POKEMON MODELS
                await Program.SetGameAsync("Loading Pokemon models");
                using (StreamReader r = new StreamReader(PokemonModel.File))
                    PokemonModels = JsonConvert.DeserializeObject<ConcurrentBag<PokemonModel>>(r.ReadToEnd());
                Console.WriteLine("Loaded " + PokemonModels.Count() + " pokemon models\n");
                //POKEMONS CATCHED
                await Program.SetGameAsync("Loading Catched Pokemons");
                using (StreamReader r = new StreamReader(PokemonEntity.File))
                    catchedPokemon = JsonConvert.DeserializeObject<ConcurrentBag<PokemonEntity>>(r.ReadToEnd());
                Console.WriteLine("Loaded " + catchedPokemon.Count() + " catched pokemons\n");
                var count = 0;
                //FIX
                foreach (var poke in catchedPokemon)
                {
                    if (poke.Moves.Count > 4)
                    {
                        poke.Moves = poke.Moves.Take(4).ToList();
                        count++;
                    }

                }
                Console.WriteLine($"Fixed {count} pokemons");
                //TRAINERS
                /*
                ConcurrentBag<Trainer> trainers = new ConcurrentBag<Trainer>();
                using (StreamReader r = new StreamReader(Trainer.File))
                     trainers = JsonConvert.DeserializeObject<ConcurrentBag<Trainer>>(r.ReadToEnd());                
                foreach (var t in trainers)
                    _trainers.AddOrUpdate(t.ID, t, (id, trainer) => trainer);*/
                using (StreamReader r = new StreamReader(Trainer.File))
                    _trainers = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, Trainer>>(r.ReadToEnd());
                //FINISH
                sp.Stop();
                Console.WriteLine("Finished after " + sp.ElapsedMilliseconds + " ms");
                await Program.SetGameAsync("guiding " + Program.pre);
                Ready = true;
            }
            catch (Exception ex)
            {
                await Program.SendReport(ex.Message, Program.ReportEnum.ERROR);
            }
            finally
            {

            }
        }
        public static async Task SaveData()
        {
            saveData = new Timer(e => Save(), null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            await Task.CompletedTask;
        }

        private static void Save()
        {

            var trainers = JsonConvert.SerializeObject(_trainers, Formatting.Indented);
            File.WriteAllText(Trainer.File, trainers);
            Console.WriteLine($"Saved {_trainers.Count} trainers");
            var catched = JsonConvert.SerializeObject(catchedPokemon, Formatting.Indented);
            File.WriteAllText(PokemonEntity.File, catched);
            Console.WriteLine($"Saved {catchedPokemon.Count} pokemons");
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
                if (move.DamageClass.Name != "special" && move.DamageClass.Name != "physical")
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
            int max = MaxPokemonId;
            List<PokemonModel> models = new List<PokemonModel>();
            for (int i = 800; i <= max; i++)
            {
                var pokemon = await DataFetcher.GetApiObject<PokeAPI.Pokemon>(i);
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
                if (pokemon.Forms.Count() > 1)
                    Console.WriteLine(pokemon.Forms.Count() + " for " + pokemon.Name);
                PokemonModel model = new PokemonModel(pokemon, idmoves);
                model.BuildIvsEvs(pokemon.Stats);
                models.Add(model);
                Console.WriteLine("[" + model.ID + "] " + model.Name);
            }
            var s = JsonConvert.SerializeObject(models, Formatting.Indented);
            File.WriteAllText("pokemon_model.json", s);
        }
        public static async Task<List<int>> GetEvolutionIDs(PokeAPI.Pokemon poke)
        {
            List<int> ids = new List<int>();
            EvolutionChain evs;
            try
            {
                evs = await DataFetcher.GetApiObject<EvolutionChain>(poke.ID);
            }
            catch (Exception) { return ids; }

            if (evs.Chain.Details.Count() > 0)
            {
                /* ids.Add(_pokemons.First(x => x.Name == evs.Chain.EvolvesTo[0].Species.Name).ID);
                  if(( l = evs.Chain.EvolvesTo[0].EvolvesTo).Count() > 0)                
                      ids.AddRange(await GetEvolutionIDs(_pokemons.First(x => x.Name == l[0].Species.Name)));  */
            }
            return ids;
        }
    }
}
