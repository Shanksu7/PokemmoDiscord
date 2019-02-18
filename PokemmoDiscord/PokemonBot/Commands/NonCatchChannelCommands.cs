using Discord.Commands;
using PokemmoDiscord.PokemonBot.Attributes;
using PokemmoDiscord.PokemonBot.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using PokemmoDiscord.PokemonBot.Entity;
using Discord;
using PokemmoDiscord.PokemonBot.Mis;

namespace PokemmoDiscord.PokemonBot.Commands
{
    [RequiereNonSpawnChannel]
    public class NonCatchChannelCommands : Comands
    {

        [Command("dex")]
        public async Task Pokedex(int id)
        {
            var poke = PokemonData.PokemonModels.First(x => x.ID == id);
            await ReplyAsync(embed: poke.EmbedBaseInformation());
        }
        [Command("dex")]
        public async Task Pokedex(string name)
        {
            var poke = PokemonData.PokemonModels.First(x => x.Name.ToLower() == name.ToLower());
            await ReplyAsync(embed: poke.EmbedBaseInformation());
        }
        [Command("$")]
        public async Task Money()
        {
            PokemonData._trainers.TryGetValue(Context.User.Id, out var trainer);
            await ReplyAsync(Context.User.Username + $" tienes **${trainer.Credits}**");
        }

        [Command("pokemon", true), Alias("p")]
        public async Task Pokemon(int page = 1)
        {
            var trainer = PokemonData._trainers.GetOrAdd(Context.User.Id, new Entity.Trainer(Context.User.Id));
            List<PokemonEntity> list = new List<PokemonEntity>();
            if ((list = trainer.Pokemons).Count() == 0)
                await ReplyAsync("No tienes pokemon");
            else
            {
                string result = "";
                int perpage = 20;
                foreach (var poke in list.Skip((list.Count > perpage) ? perpage * (page - 1) : 0).Take(perpage))
                    result += $"{list.IndexOf(poke) + 1} - {poke.Nickname}, lvl:{poke.Level}, {poke.IVPercent}% {(poke.Shiny ? "**Shiny**" : "")}\n";
                await ReplyAsync(result);
            }
        }
        [Command("pokemon", true), Alias("p")]
        public async Task Pokemon(string name, int page = 1)
        {
            var trainer = PokemonData._trainers.GetOrAdd(Context.User.Id, new Entity.Trainer(Context.User.Id));
            List<PokemonEntity> sortedlist = new List<PokemonEntity>();
            List<PokemonEntity> list = trainer.Pokemons;
            if ((sortedlist = list.Where(x => x.Nickname.Contains(name)).ToList()).Count() == 0)
                await ReplyAsync($"No tienes pokemon que contengan **{name}** en su nombre");
            else
            {
                string result = "";
                int perpage = 20;
                foreach (var poke in sortedlist.Skip((sortedlist.Count > perpage) ? perpage * (page - 1) : 0).Take(perpage))
                    result += $"{list.IndexOf(poke) + 1} - {poke.Nickname}, lvl:{poke.Level}, {poke.IVPercent}% {(poke.Shiny ? "**Shiny**" : "")}\n";
                await ReplyAsync(result);
            }
        }
        [Command("select", true)]
        public async Task Select(int ind)
        {
            //bajamos el indice
            ind--;
            var trainer = PokemonData._trainers.GetOrAdd(Context.User.Id, new Entity.Trainer(Context.User.Id));
            var list = trainer.Pokemons;
            if (ind < list.Count)
            {
                var poke = trainer.SetSelectedPokemon(ind);
                await ReplyAsync($"U has selected to **{poke.Nickname}** lvl:{poke.Level}");
            }

        }
        [Command("pinfo", true), Alias("pi")]
        public async Task Info()
        {
            var target = Context.Message.MentionedUsers.FirstOrDefault() ?? Context.User;
            var trainer = PokemonData._trainers.GetOrAdd(target.Id, new Entity.Trainer(target.Id));
            if (trainer.Pokemons.Count > 0)
            {
                var p = trainer.SelectedPokemon;
                await ReplyAsync(embed: p.EmbedInformation());
            }
        }
        [Command("heal", true), Alias("h")]
        public async Task Heal()
        {
            var target = Context.Message.MentionedUsers.FirstOrDefault() ?? Context.User;
            var trainer = PokemonData._trainers.GetOrAdd(target.Id, new Entity.Trainer(target.Id));
            if (trainer.Pokemons.Count > 0)
            {
                var p = trainer.SelectedPokemon;
                if (p.Heal(9999))
                    await ReplyAsync($"**{target.Username}'s {p.Nickname}** has been **Healed**!");
                else await ReplyAsync($"**{target.Username}'s {p.Nickname}** is full!");
            }
        }
        [Command("ivs", true)]
        public async Task InfoIvs()
        {
            var trainer = PokemonData._trainers.GetOrAdd(Context.User.Id, new Entity.Trainer(Context.User.Id));
            if (trainer.Pokemons.Count > 0)
            {
                var p = trainer.SelectedPokemon;
                await ReplyAsync(embed: p.EmbedIVsInformation());
            }
        }
        [Command("learn", true)]
        public async Task LearnMove(int idmove, int id)
        {
            var trainer = PokemonData._trainers.GetOrAdd(Context.User.Id, new Entity.Trainer(Context.User.Id));
            try
            {
                var move = PokemonData.MovesModel.First(x => x.ID == idmove);

                if (id <= 4 && !trainer.SelectedPokemon.Moves.Contains(move.ID) && trainer.SelectedPokemon.Model.AvailableMoveIDS.Contains(move.ID))
                {
                    if (trainer.SelectedPokemon.Moves.Count < 4)
                        trainer.SelectedPokemon.Moves.Add(move.ID);
                    else
                        trainer.SelectedPokemon.Moves[id - 1] = move.ID;
                }
                else await ReplyAsync($"Parece que no puedes aprender **{move.ESPName}**..! intenta de nuevo :)");
            }
            catch
            {
                await ReplyAsync("El modo de uso es .learn <id de MT> <#slot>\nEjemplo para aprender puño cometa en la posicion 1 `.learn 4 1`", embed: trainer.SelectedPokemon.EmbedMovesInformation());

            }
        }
        [Command("learn", true)]
        public async Task LearnMove()
        {
            var trainer = PokemonData._trainers.GetOrAdd(Context.User.Id, new Entity.Trainer(Context.User.Id));
            await ReplyAsync("El modo de uso es .learn <id de MT> <#slot>\nEjemplo para aprender puño cometa en la posicion 1 `.learn 4 1`", embed: trainer.SelectedPokemon.EmbedMovesInformation());
        }
        [Command("fconfirm", true)]
        public async Task ConfirmFight()
        {
            if (PokemonData._trainers.TryGetValue(Context.User.Id, out var trainer))
            {
                if (trainer.Fight.State == FightState.REQUESTING)
                {
                    await trainer.Fight.StartFight();
                }
            }
        }

        [Command("fight", true), Alias("f")]
        public async Task Fight()
        {
            if (Context.Message.MentionedUsers.Count > 0)
            {
                var target = Context.Message.MentionedUsers.First();
                if (target.IsBot)
                    return;
                if (PokemonData._trainers.TryGetValue(target.Id, out var defender) &&
                    PokemonData._trainers.TryGetValue(Context.User.Id, out var attacker))
                {

                    if (attacker.InFight || defender.InFight)
                    {
                        await ReplyAsync("Deben estar fuera de combate");
                        return;
                    }
                    if (defender.SelectedPokemon.RemainingPS == 0 || attacker.SelectedPokemon.RemainingPS == 0)
                    {
                        await ReplyAsync("Revisa que tu pokemon o el de tu oponente tengan **PS** disponibles");
                        return;
                    }

                    if (defender.SelectedPokemon.Moves.Count == 0 || attacker.SelectedPokemon.Moves.Count == 0)
                    {
                        await ReplyAsync("**Ambos** entrenadores necesitan aprender almenos un movimiento");
                        return;
                    }
                    EmbedBuilder em = new EmbedBuilder();
                    em.WithTitle("Espera un momento");
                    em.WithDescription("Confirmando duelo, 30 segundos para empezar . . . ^^");
                    Embed embed = em.Build();
                    var t = await Context.Channel.SendMessageAsync($"{target.Mention} usa .fconfirm para aceptar el duelo", embed: embed);
                    var t2 = await Context.User.SendMessageAsync(embed: embed);
                    var t3 = await target.SendMessageAsync(embed: embed);
                    var Fight = new PokemonFight(attacker.SelectedPokemon, defender.SelectedPokemon, t, t2, t3);
                    attacker.Fight = Fight;
                    defender.Fight = Fight;
                    //await Fight.StartFight();                    
                }
                else
                    await ReplyAsync("Parece que no tiene ningun pokemon");
            }
            else
                await ReplyAsync("Debes mencionar a un entrenador");

        }

        [Command("if", true)]
        public async Task IsInFight()
        {
            PokemonData._trainers.TryGetValue(Context.User.Id, out var attacker);
            await ReplyAsync((attacker.InFight) ? "Si" : "No");

        }
    }
}
