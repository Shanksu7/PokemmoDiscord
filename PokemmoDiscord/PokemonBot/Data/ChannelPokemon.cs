using AKDiscordBot;
using System.Linq;
using AKDiscordBot.Extensions;
using Discord.WebSocket;
using PokemmoDiscord.PokemonBot.Entity;
using PokemmoDiscord.PokemonBot.Mis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PokemmoDiscord.PokemonBot.Data
{

    class ChannelPokemon
    {
        public ChannelPokemon(GenerationEnum gen, ulong chid, int start, int end, List<int> nonspawneable)
        {
            ChId = chid;
            StartIndex = start;
            EndIndex = end;
            NonSpawneable = nonspawneable;
        }
        public GenerationEnum Generation { get; set; }
        public ulong ChId { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public List<int> NonSpawneable { get; set; }
        public int GetRandomID()
        {
            Random r = new Random(Environment.TickCount.GetHashCode());
            int id = r.Next(StartIndex, EndIndex + 1);
            while (NonSpawneable.Contains(id))
                id = r.Next(StartIndex, EndIndex + 1);
            return id;
        }


        //STATIC
        public static List<ChannelPokemon> ChannelsToSpawn
        {
            get
            {
                return new List<ChannelPokemon>()
                {
                    new ChannelPokemon(GenerationEnum.I, 542132177291313153, 1,   151, new List<int>(){ 151,150,146,145,144,}),
                    new ChannelPokemon(GenerationEnum.II,542132195154984961, 152, 251, new List<int>(){  251, 250, 249, 245, 244, 243,201}),
                    new ChannelPokemon(GenerationEnum.III,542132185797230592, 252, 386, new List<int>(){ 386,385,384,383,382,381,380,379,378}),
                    new ChannelPokemon(GenerationEnum.IV,542132169347170304, 387, 493, new List<int>(){ 447,448,479,480,481,482,483,484,485,486,487,488,489,490,491,492,493 }),
                    new ChannelPokemon(GenerationEnum.V,542132160656703491, 494, 649, new List<int>(){ 637,638,639,640,641,642,643,644,645,646,647,648,649 }),
                    new ChannelPokemon(GenerationEnum.VI,542132105291759626, 650, 721, new List<int>(){ 716,717,718,719,720,721}),
                    new ChannelPokemon(GenerationEnum.VII,543154800171286528, 722, 802, new List<int>(){ 785,786,787,788,789,790,791,792,793,794,795,796,797,798,799,800,801,802})


                };
            }
        }

        public static ConcurrentDictionary<ulong, PokemonEntity> PokemonInChannel = new ConcurrentDictionary<ulong, PokemonEntity>();
        private static Timer spawnPool;
        public static async Task StartSpawn()
        {
            spawnPool = new Timer(e => Spawn(), null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            await Task.CompletedTask;
        }
        private static async void Spawn()
        {
            if (!PokemonData.Ready)
                return;
            foreach (var ch in ChannelsToSpawn)
            {
                var channel = Program.Client.GetChannel(ch.ChId) as SocketTextChannel;
                var test = channel.Users.Where(x => !x.IsBot).ToList();
                var Admins = RequireAdminIdAttribute.AllowedIDS.Count;
                if (test.Count <= (Admins + 1)) //admins + bot
                    continue;
                PokemonEntity newPokemon = new PokemonEntity(ch.GetRandomID());
                PokemonInChannel.AddOrUpdate(ch.ChId, newPokemon, (chid, poke) => poke = newPokemon);
                await channel.SendMessageAsync(embed: newPokemon.EmbedWildInformation());
                Console.WriteLine($"Spawned {newPokemon.Nickname} [{newPokemon.ID}] at '{channel.Name}'");
            }
        }
    }
}
