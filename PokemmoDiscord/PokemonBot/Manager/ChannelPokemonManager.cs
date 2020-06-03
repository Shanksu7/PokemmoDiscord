using AKDiscordBot;
using AKDiscordBot.Extensions;
using Discord.WebSocket;
using PokemmoDiscord.PokemonBot.Data;
using PokemmoDiscord.PokemonBot.Entity;
using PokemmoDiscord.PokemonBot.Mis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace PokemmoDiscord.PokemonBot.Manager
{
    public class ChannelPokemonManager
    {
        private static Timer spawnPool;
        public static List<ChannelPokemon> ChannelsToSpawn
        {
            get
            {
                return new List<ChannelPokemon>()
                {
                    new ChannelPokemon(GenerationEnum.I, ChannelEnum.KANTO, 1,   151, new List<int>(){ 151,150,146,145,144,}),
                    new ChannelPokemon(GenerationEnum.II,ChannelEnum.JOHTO, 152, 251, new List<int>(){  251, 250, 249, 245, 244, 243,201}),
                    new ChannelPokemon(GenerationEnum.III,ChannelEnum.HOENN, 252, 386, new List<int>(){ 386,385,384,383,382,381,380,379,378}),
                    new ChannelPokemon(GenerationEnum.IV,ChannelEnum.SINNOH, 387, 493, new List<int>(){ 447,448,479,480,481,482,483,484,485,486,487,488,489,490,491,492,493 }),
                    new ChannelPokemon(GenerationEnum.V,ChannelEnum.UNOVA, 494, 649, new List<int>(){ 637,638,639,640,641,642,643,644,645,646,647,648,649 }),
                    new ChannelPokemon(GenerationEnum.VI,ChannelEnum.KALOS, 650, 721, new List<int>(){ 716,717,718,719,720,721}),
                    new ChannelPokemon(GenerationEnum.VII,ChannelEnum.ALOLA, 722, 802, new List<int>(){ 785,786,787,788,789,790,791,792,793,794,795,796,797,798,799,800,801,802})
                };
            }
        }

        public static ConcurrentDictionary<ulong, PokemonEntity> PokemonInChannel = new ConcurrentDictionary<ulong, PokemonEntity>();

        public static async Task StartSpawn()
        {
            spawnPool = new Timer()
            {
                Interval = TimeSpan.FromSeconds(60).TotalMilliseconds,
                AutoReset = true,
            };
            spawnPool.Elapsed += OnSpawn;
            spawnPool.Start();
            await Task.CompletedTask.ConfigureAwait(false);
        }

        private static async void OnSpawn(object sender, ElapsedEventArgs e)
        {
            await Spawn().ConfigureAwait(false);
        }

        private static async Task Spawn()
        {
            if (!PokemonData.Ready)
                return;
            foreach (var ch in ChannelsToSpawn)
            {
                var channel = Program.Client.GetChannel((ulong)ch.ChId) as SocketTextChannel;
                if (channel == null)
                {
                    Console.WriteLine("Channel [" + ch.ChId + "] : " + channel.Id + " is wrong");
                    continue;
                }
                var test = channel.Users.Where(x => !x.IsBot).ToList();
                var Admins = RequireAdminIdAttribute.AllowedIDS.Count;

                if (test.Count <= (Admins + 1)) //admins + bot
                    continue;

                PokemonEntity newPokemon = new PokemonEntity(ch.GetRandomPkmnID());
                PokemonInChannel.AddOrUpdate((ulong)ch.ChId, newPokemon, (chid, poke) => poke = newPokemon);
                await channel.SendMessageAsync(embed: newPokemon.EmbedWildInformation()).ConfigureAwait(false);
                Console.WriteLine($"Spawned {newPokemon.Nickname} [{newPokemon.ID}] at '{channel.Name}'");
            }
        }
    }
}
