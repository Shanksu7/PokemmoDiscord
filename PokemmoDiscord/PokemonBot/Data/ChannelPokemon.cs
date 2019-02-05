using AKDiscordBot;
using Discord.WebSocket;
using PokemmoDiscord.PokemonBot.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PokemmoDiscord.PokemonBot.Data
{
    class ChannelPokemon
    {
        //channelid , pokemon
        public static List<ulong> ChannelsToSpawn
        {
            get
            {
                return new List<ulong>()
                {              
                    542132195154984961,
                    542132185797230592,
                    542132169347170304,
                    542132160656703491,
                    542132105291759626,
                    542132177291313153
                };
            }
        }
        public static List<int> NonSpawneablePokemon
        {
            get
            {
                return new List<int>()
                {
                    488, 799, 491,
                    
                };
            }
        }
        private static ConcurrentDictionary<ulong, Pokemon> PokemonInChannel = new ConcurrentDictionary<ulong, Pokemon>();
        private static Timer spawnPool;
        public static async Task Start()
        {
            spawnPool = new Timer(e => Spawn(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            await Task.CompletedTask;
        }
        private static async void Spawn()
        {
            if (!PokemonData.Ready)
                return;
            foreach(var ch in ChannelsToSpawn)
            {
                var channel = Program.Client.GetChannel(ch);
                var test = channel.Users;
                if (test.Count <= 2)
                    continue;
                Random r = new Random(Environment.TickCount.GetHashCode());
                int id = r.Next(PokemonData.MaxPokemonId) + 1;
                while(NonSpawneablePokemon.Contains(id))
                    id = r.Next(PokemonData.MaxPokemonId) + 1;
                Pokemon newPokemon = new Pokemon(id);
                PokemonInChannel.AddOrUpdate(ch,newPokemon , (chid, poke) => poke = newPokemon);                
                await ( channel as SocketTextChannel).SendMessageAsync(embed:newPokemon.EmbedWildInformation());
            }
        }
    }
}
