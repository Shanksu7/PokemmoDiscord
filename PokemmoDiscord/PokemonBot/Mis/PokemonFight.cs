using Discord;
using Discord.Rest;
using Discord.WebSocket;
using PokemmoDiscord.PokemonBot.Data;
using PokemmoDiscord.PokemonBot.Entity;
using PokemmoDiscord.PokemonBot.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PokemmoDiscord.PokemonBot.Mis
{
    public class PokemonFight 
    {
        public PokemonFight(PokemonEntity atkpkm, PokemonEntity defpkm, RestUserMessage chmsg, IUserMessage atk1, IUserMessage def2)
        {
            Attacker = atkpkm;
            Defender = defpkm;
            AttackerMessage = atk1;
            DefenderMessage = def2;
            ChannelMessage = chmsg;
            State = FightState.REQUESTING;
        }
        public PokemonEntity Attacker { get; set; }
        public PokemonEntity Defender { get; set; }
        public IUserMessage AttackerMessage { get; set; }
        public IUserMessage DefenderMessage { get; set; }
        public MoveModel AttackerMove { get; set; }
        public MoveModel DefenderMove { get; set; }
        public RestUserMessage ChannelMessage { get; set; }
        private Timer Clock { get; set; }
        public FightState State { get; set; }
        public async Task StartFight()
        {
            Attacker.Trainer.Fight.State = FightState.RUNNING;
            Defender.Trainer.Fight.State = FightState.RUNNING;
            Clock = new Timer(e => EndFight().GetAwaiter(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(3));            
            await Update(EmbedAttackerPerspective, EmbedDefenderPerspective, EmbedChannelPerspective);
        }
        public async Task RequestingDuel()
        {
            Clock = new Timer(e => CancelRequest().GetAwaiter(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(3));
            await Task.CompletedTask;
        }

        private async Task CancelRequest()
        {
            if (State != FightState.RUNNING)
            {
                Attacker.Trainer.Fight.State = FightState.FINISHED;
                Defender.Trainer.Fight.State = FightState.FINISHED;
                Embed embed = new EmbedBuilder()
                    .WithTitle("Cancelado!")
                    .WithDescription("Se ha terminado el tiempo de espera")
                    .Build();
                await Update(embed, embed, embed);
                await Task.CompletedTask;
            }
        }

        public async Task SetMovement(ulong id, int moveid)
        {
            try
            {   
                if (Attacker.OwnerID == id && AttackerMove == null)
                    AttackerMove = Attacker.MovesModels[moveid];                
                else if(Defender.OwnerID == id && DefenderMove == null)
                    DefenderMove = Defender.MovesModels[moveid];
                if(AttackerMove != null && DefenderMove != null)
                {
                    await DoDamage();
                    AttackerMove = null;
                    DefenderMove = null;
                    Clock = new Timer(e => EndFight().GetAwaiter(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(3));
                }
            }
            catch
            {
                
            }
            await Task.CompletedTask;
        }

        private async Task DoDamage()
        {
            var dmg1 = AttackerMove.CalculateDamage(Attacker, Defender);
            var dmg2 = DefenderMove.CalculateDamage(Defender, Attacker);
            await Attacker.Trainer.DiscordUser.SendMessageAsync($"Tu {Attacker.Nickname} uso el movimiento {AttackerMove.Name} e hizo {dmg1} de daño.");
            await Defender.Trainer.DiscordUser.SendMessageAsync($"Tu {Defender.Nickname} uso el movimiento {DefenderMove.Name} e hizo {dmg2} de daño.");
            if (Attacker.DealDamage(dmg2) || Defender.DealDamage(dmg1))
            {
                await EndFight();
                return;
            }            
            await Update(EmbedAttackerPerspective, EmbedDefenderPerspective, EmbedChannelPerspective);
        }

        private async Task EndFight()
        {
            var result = new ConcurrentDictionary<Perspective, Embed>();// BuildResultPerspective(true);            
            result = BuildResultPerspective(Attacker.RemainingPS > Defender.RemainingPS);
            await Update(result);
            Clock.Dispose();
            State = FightState.FINISHED;
        }

        public async Task Update(Embed atkmodel, Embed defmodel, Embed chmodel)
        {
            await AttackerMessage.ModifyAsync(e => e.Embed = atkmodel);
            await DefenderMessage.ModifyAsync(e => e.Embed = defmodel);
            await ChannelMessage.ModifyAsync(e => e.Embed = chmodel);
        }
        public async Task Update(ConcurrentDictionary<Perspective, Embed> results)
        {
            await AttackerMessage.ModifyAsync(e => e.Embed = results[Perspective.ATTACKER]);
            await DefenderMessage.ModifyAsync(e => e.Embed = results[Perspective.DEFENDER]);
            await ChannelMessage.ModifyAsync(e => e.Embed = results[Perspective.CHANNEL]);
        }
        public Embed EmbedChannelPerspective => BuildBattlePerspective(Attacker, Defender, false);
        public Embed EmbedAttackerPerspective => BuildBattlePerspective(Attacker, Defender);
        public Embed EmbedDefenderPerspective => BuildBattlePerspective(Defender, Attacker);
        public Embed BuildBattlePerspective(PokemonEntity main, PokemonEntity second, bool to_user = true)
        {
            EmbedBuilder eb = new EmbedBuilder();
            var atkmodel = main.Model;
            var defmodel = second.Model;            
            eb.WithThumbnailUrl(second.Front);
            eb.WithImageUrl(main.Front);
            eb.WithTitle(second.Nickname + $" PS: ({second.RemainingPS}/{second.Stats[StatTypeEnum.HP]})");
            eb.WithFooter(main.Nickname + $" PS: ({main.RemainingPS}/{main.Stats[StatTypeEnum.HP]})");
            eb.WithColor(atkmodel.GetColor());
            if (to_user)
            {
                string mr = "";
                var count = 1;
                foreach (var move in main.MovesModels)
                    mr += count++ + $") {move.Name}";                
                eb.WithDescription(mr);
            }
            Embed embed = eb.Build();
            return embed;
        }
        public ConcurrentDictionary<Perspective, Embed> BuildResultPerspective(bool attackerWins)
        {
            ConcurrentDictionary<Perspective, Embed> result = new ConcurrentDictionary<Perspective, Embed>();
            Embed deb = null, aeb = null, cheb = null;
            Random r = new Random(DateTime.Now.GetHashCode());
            var basemoney = r.Next(10, 25) + (r.Next(1, 9) /10);
            if (attackerWins)
            {
                Attacker.GiveExperience(Defender);
                Attacker.Trainer.GiveCredits(basemoney * Defender.Level);
                deb = LoserEmbed(Defender);
                aeb = WinnerEmbed(Attacker);
                cheb = ChannelEmbed(Attacker, Defender);                
            }
            else
            {
                Defender.GiveExperience(Attacker);
                Defender.Trainer.GiveCredits(basemoney * Attacker.Level);
                deb = WinnerEmbed(Defender);
                aeb = LoserEmbed(Attacker);
                cheb = ChannelEmbed(Defender, Attacker);                
            }
            result.TryAdd(Perspective.ATTACKER, aeb);
            result.TryAdd(Perspective.DEFENDER, deb);
            result.TryAdd(Perspective.CHANNEL, cheb);
            return result;
        }        
        public Embed WinnerEmbed(PokemonEntity pkmn)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle("¡Ganaste!");
            eb.WithImageUrl(pkmn.Model.LargeFront);
            eb.AddField($"{pkmn.Nickname} Lvl. {pkmn.Level}", $"EXP: {pkmn.CurrentExperience} / {pkmn.NeededExperience}");
            eb.AddField($"Dinero Actual", "$"+pkmn.Trainer.Credits);
            eb.WithColor(Color.Green);
            Embed embed = eb.Build();
            
            return embed;                
        }
        public Embed LoserEmbed(PokemonEntity pkmn)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithTitle("¡Perdiste!");
            eb.WithImageUrl(pkmn.Model.LargeFront);
            eb.AddField($"{pkmn.Nickname} Lvl. {pkmn.Level}", $"EXP: {pkmn.CurrentExperience} / {pkmn.NeededExperience}");
            eb.WithColor(Color.Red);
            Embed embed = eb.Build();

            return embed;
        }
        public Embed ChannelEmbed(PokemonEntity winner, PokemonEntity loser)
        {
            EmbedBuilder eb = new EmbedBuilder();
            PokemonData._trainers.TryGetValue(winner.OwnerID, out var trainer);
            PokemonData._trainers.TryGetValue(loser.OwnerID, out var trainer2);
            eb.WithTitle($"¡Ganador {winner.Nickname} Lvl. {winner.Level}!");
            eb.WithImageUrl(winner.Model.LargeFront);
            eb.AddField($"Entrenador Victorioso", trainer.DiscordUser.Username + "#" + trainer.DiscordUser.Discriminator);
            eb.AddField($"Entrenador Perdedor", trainer2.DiscordUser.Username + "#" + trainer2.DiscordUser.Discriminator);
            eb.WithColor(Color.Green);
            Embed embed = eb.Build();

            return embed;
        }
    }
    public enum Perspective
    {
        ATTACKER,
        DEFENDER,
        CHANNEL
    }
    public enum FightState
    {
        REQUESTING,
        RUNNING,
        FINISHED        
    }
}
