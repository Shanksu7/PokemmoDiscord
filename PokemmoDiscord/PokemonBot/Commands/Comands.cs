using AKDiscordBot.Extensions;
using Discord.Commands;
using PokeAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using PokemmoDiscord.PokemonBot.Attributes;
using PokemmoDiscord.PokemonBot.Data;
using PokemmoDiscord.PokemonBot.Entity;
using Discord;

namespace PokemmoDiscord.PokemonBot.Commands
{
    public class Comands : ModuleBase<SocketCommandContext>
    {
        [Command("roles"), RequireAdminId]
        public async Task Roles()
        {
            var result = "";
            foreach (var role in Context.Guild.Roles)
                result += role.Mention + " " + role.Id + "\n";
            await ReplyAsync(result);

        }
        [Command("dex"), RequireAdminId]
        public async Task Pokedex(int id)
        {
            var poke = PokemonData.PokemonModels.First(x => x.ID == id);
            await ReplyAsync(embed: poke.EmbedBaseInformation());
        }
        [Command("dex"), RequireAdminId]
        public async Task Pokedex(string name)
        {
            var poke = PokemonData.PokemonModels.First(x => x.Name.ToLower() == name.ToLower());
            await ReplyAsync(embed: poke.EmbedBaseInformation());
        }
        [Command("catch", true), RequiereSpawnChannel]
        public async Task Catch(string name)
        {
            if (ChannelPokemon.PokemonInChannel.TryGetValue(Context.Channel.Id, out var poke))
                if (poke.Nickname.ToLower() == name.ToLower())
                {
                    ChannelPokemon.PokemonInChannel.Remove(Context.Channel.Id, out var poker);
                    poke.SetOwner(Context.User.Id);
                    PokemonModel model = poke.GetModel();
                    EmbedBuilder eb = new EmbedBuilder();
                    eb.WithTitle($"Congratz! You catched a \n**{model.Name.ToUpper()}** \t [#{model.ID}]");
                    eb.WithThumbnailUrl(model.GetLargeFront());
                    eb.AddField("Lvl", poke.Lvl, true);
                    eb.AddField("IVs", poke.GetIVPercent() + "%", true);
                    eb.WithColor(model.GetColor());
                    var embed = eb.Build();
                    await ReplyAsync(embed: embed);
                }
                else
                    await ReplyAsync("That's not the name of the pokemon try again");
        }
        [Command("pokemon", true), Alias("p")]
        public async Task Pokemon(int page = 1)
        {
            var trainer = PokemonData._trainers.GetOrAdd(Context.User.Id, new Entity.Trainer(Context.User.Id));
            List<PokemonEntity> list = new List<PokemonEntity>();
            if ((list = trainer.GetPokemons()).Count() == 0)
                await ReplyAsync("No tienes pokemon");
            else
            {
                string result = "";
                int perpage = 20;
                foreach (var poke in list.Skip((list.Count > perpage) ? perpage * (page - 1) : 0).Take(perpage))
                    result += $"{list.IndexOf(poke) + 1} - {poke.Nickname}, lvl:{poke.Lvl}, {poke.GetIVPercent()}%\n";
                await ReplyAsync(result);
            }
        }
        [Command("pokemon", true), Alias("p")]
        public async Task Pokemon(string name, int page = 1)
        {
            var trainer = PokemonData._trainers.GetOrAdd(Context.User.Id, new Entity.Trainer(Context.User.Id));
            List<PokemonEntity> sortedlist = new List<PokemonEntity>();
            List<PokemonEntity> list = trainer.GetPokemons();
            if ((sortedlist = list.Where(x => x.Nickname.Contains(name)).ToList()).Count() == 0)
                await ReplyAsync($"No tienes pokemon que contengan **{name}** en su nombre");
            else
            {
                string result = "";
                int perpage = 20;
                foreach (var poke in sortedlist.Skip((sortedlist.Count > perpage) ? perpage * (page - 1) : 0).Take(perpage))
                    result += $"{list.IndexOf(poke) + 1} - {poke.Nickname}, lvl:{poke.Lvl}, {poke.GetIVPercent()}%\n";
                await ReplyAsync(result);
            }
        }
        [Command("select", true)]
        public async Task Select(int ind)
        {
            //bajamos el indice
            ind--;
            var trainer = PokemonData._trainers.GetOrAdd(Context.User.Id, new Entity.Trainer(Context.User.Id));
            var list = trainer.GetPokemons();
            if (ind < list.Count)
            {
                var poke = trainer.SetSelectedPokemon(ind);
                await ReplyAsync($"U has selected to **{poke.Nickname}** lvl:{poke.Lvl}");
            }

        }
        [Command("pinfo", true), Alias("pi")]
        public async Task Info()
        {
            var trainer = PokemonData._trainers.GetOrAdd(Context.User.Id, new Entity.Trainer(Context.User.Id));
            if (trainer.GetPokemons().Count > 0)
            {
                var p = trainer.GetSelectedPokemon();
                await ReplyAsync(embed: p.EmbedInformation());
            }
        }
        [Command("ivs", true)]
        public async Task InfoIvs()
        {
            var trainer = PokemonData._trainers.GetOrAdd(Context.User.Id, new Entity.Trainer(Context.User.Id));
            if (trainer.GetPokemons().Count > 0)
            {
                var p = trainer.GetSelectedPokemon();
                await ReplyAsync(embed: p.EmbedIVsInformation());
            }
        }
        [Command("test", true), RequireAdminId]
        public async Task Test()
        {
            EmbedBuilder eb = new EmbedBuilder();
            try
            {
                
                eb.WithTitle($"Congratz!");
                eb.WithImageUrl("http://i.imgur.com/pGCM1mZ.jpg");
                eb.WithDescription("lol arceus");
                eb.WithThumbnailUrl("https://sites.google.com/site/aurakingdomguia/_/rsrc/1479963432606/trabajos/recoleccion/ingredientes/ingpolvoamarillo.jpg");
                eb.WithColor(Color.Red);
                var embed = eb.Build();
                await ReplyAsync(embed: embed);
            }
            catch(Exception ex)
            {
                eb.WithTitle($"Congratz! You catched a bug");
                eb.WithDescription(ex.Message);
                var embed = eb.Build();
                await ReplyAsync(embed: embed);
            }
        }
    }
}
