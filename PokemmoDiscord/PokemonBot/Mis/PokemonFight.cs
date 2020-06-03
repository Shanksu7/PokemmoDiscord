using Discord;
using Discord.Rest;
using PokemmoDiscord.PokemonBot.Data;
using PokemmoDiscord.PokemonBot.Entity;
using PokemmoDiscord.PokemonBot.Models;
using System;
using System.Collections.Concurrent;
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

        public Embed EmbedChannelPerspective => BuildBattlePerspective(Attacker, Defender, false);
        public Embed EmbedAttackerPerspective => BuildBattlePerspective(Attacker, Defender);
        public Embed EmbedDefenderPerspective => BuildBattlePerspective(Defender, Attacker);

        public async Task StartFight()
        {
            Attacker.Trainer.Fight.State = FightState.RUNNING;
            Defender.Trainer.Fight.State = FightState.RUNNING;
            Clock = new Timer(e => EndFight().GetAwaiter(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(3));
            await Update(EmbedAttackerPerspective, EmbedDefenderPerspective, EmbedChannelPerspective).ConfigureAwait(false);
        }
        public async Task RequestingDuel()
        {
            Clock = new Timer(async e => await CancelRequest().ConfigureAwait(false), null, TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(-1));
            await Task.CompletedTask;
        }

        public async Task CancelRequest()
        {
            if (State != FightState.RUNNING)
            {
                Attacker.Trainer.Fight.State = FightState.FINISHED;
                Defender.Trainer.Fight.State = FightState.FINISHED;
                Embed embed = new EmbedBuilder()
                    .WithTitle("Cancelado!")
                    .WithDescription("Se ha terminado el tiempo de espera")
                    .Build();
                await Update(embed, embed, embed).ConfigureAwait(false);
            }
        }

        public async Task SetMovement(ulong id, int moveid)
        {
            try
            {
                if (Attacker.OwnerID == id && AttackerMove == null)
                    AttackerMove = Attacker.MovesModels[moveid];
                else if (Defender.OwnerID == id && DefenderMove == null)
                    DefenderMove = Defender.MovesModels[moveid];
                if (AttackerMove != null && DefenderMove != null)
                {
                    await DoDamage().ConfigureAwait(false);
                    AttackerMove = null;
                    DefenderMove = null;
                    Clock = new Timer(async e => await EndFight().ConfigureAwait(false), null, TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(-1));
                }
            }
            catch
            {

            }
        }

        private async Task DoDamage()
        {
            var dmg1 = AttackerMove.CalculateDamage(Attacker, Defender);
            var dmg2 = DefenderMove.CalculateDamage(Defender, Attacker);
            await Attacker.Trainer.DiscordUser.SendMessageAsync($"Tu {Attacker.Nickname} uso el movimiento {AttackerMove.Name} e hizo {dmg1} de daño.").ConfigureAwait(false);
            await Defender.Trainer.DiscordUser.SendMessageAsync($"Tu {Defender.Nickname} uso el movimiento {DefenderMove.Name} e hizo {dmg2} de daño.").ConfigureAwait(false);
            if (Attacker.DealDamage(dmg2) || Defender.DealDamage(dmg1))
            {
                await EndFight().ConfigureAwait(false);
                return;
            }
            await Update(EmbedAttackerPerspective, EmbedDefenderPerspective, EmbedChannelPerspective).ConfigureAwait(false);
        }

        private async Task EndFight()
        {
            var result = BuildResultPerspective(Attacker.RemainingPS > Defender.RemainingPS);
            await Update(result).ConfigureAwait(false);
            Clock.Dispose();
            State = FightState.FINISHED;
        }

        public async Task Update(Embed atkmodel, Embed defmodel, Embed chmodel)
        {
            await AttackerMessage.ModifyAsync(e => e.Embed = atkmodel).ConfigureAwait(false);
            await DefenderMessage.ModifyAsync(e => e.Embed = defmodel).ConfigureAwait(false);
            await ChannelMessage.ModifyAsync(e => e.Embed = chmodel).ConfigureAwait(false);
        }

        public async Task Update(ConcurrentDictionary<Perspective, Embed> results)
        {
            await AttackerMessage.ModifyAsync(e => e.Embed = results[Perspective.ATTACKER]).ConfigureAwait(false);
            await DefenderMessage.ModifyAsync(e => e.Embed = results[Perspective.DEFENDER]).ConfigureAwait(false);
            await ChannelMessage.ModifyAsync(e => e.Embed = results[Perspective.CHANNEL]).ConfigureAwait(false);
        }
        public Embed BuildBattlePerspective(PokemonEntity main, PokemonEntity second, bool to_user = true)
        {
            var eb = new EmbedBuilder();
            var atkmodel = main.GetModel();
            //var defmodel = second.GetModel();            
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
            return eb.Build();
        }

        public ConcurrentDictionary<Perspective, Embed> BuildResultPerspective(bool attackerWins)
        {
            var result = new ConcurrentDictionary<Perspective, Embed>();
            Embed def_eb = null, atk_eb = null, ch_eb = null;
            var r = new Random(DateTime.Now.GetHashCode());
            var basemoney = r.Next(10, 25) + (r.Next(1, 9) / 10);

            if (attackerWins)
            {
                Attacker.GiveExperience(Defender);
                Attacker.Trainer.GiveCredits(basemoney * Defender.Level);
                def_eb = LoserEmbed(Defender);
                atk_eb = WinnerEmbed(Attacker);
                ch_eb = ChannelEmbed(Attacker, Defender);
            }
            else
            {
                Defender.GiveExperience(Attacker);
                Defender.Trainer.GiveCredits(basemoney * Attacker.Level);
                def_eb = WinnerEmbed(Defender);
                atk_eb = LoserEmbed(Attacker);
                ch_eb = ChannelEmbed(Defender, Attacker);
            }
            result.TryAdd(Perspective.ATTACKER, atk_eb);
            result.TryAdd(Perspective.DEFENDER, def_eb);
            result.TryAdd(Perspective.CHANNEL, ch_eb);
            return result;
        }

        public Embed WinnerEmbed(PokemonEntity pkmn)
        {
            var eb = new EmbedBuilder();
            eb.WithTitle("¡Ganaste!");
            eb.WithImageUrl(pkmn.GetModel().LargeFront);
            eb.AddField($"{pkmn.Nickname} Lvl. {pkmn.Level}", $"EXP: {pkmn.CurrentExperience} / {pkmn.NeededExperience}");
            eb.AddField($"Dinero Actual", "$" + pkmn.Trainer.Credits);
            eb.WithColor(Color.Green);
            return eb.Build();
        }

        public Embed LoserEmbed(PokemonEntity pkmn)
        {
            var eb = new EmbedBuilder();
            eb.WithTitle("¡Perdiste!");
            eb.WithImageUrl(pkmn.GetModel().LargeFront);
            eb.AddField($"{pkmn.Nickname} Lvl. {pkmn.Level}", $"EXP: {pkmn.CurrentExperience} / {pkmn.NeededExperience}");
            eb.WithColor(Color.Red);
            return eb.Build();
        }

        public Embed ChannelEmbed(PokemonEntity winner, PokemonEntity loser)
        {
            var eb = new EmbedBuilder();
            PokemonData._trainers.TryGetValue(winner.OwnerID, out var trainer);
            PokemonData._trainers.TryGetValue(loser.OwnerID, out var trainer2);
            eb.WithTitle($"¡Ganador {winner.Nickname} Lvl. {winner.Level}!");
            eb.WithImageUrl(winner.GetModel().LargeFront);
            eb.AddField($"Entrenador Victorioso", trainer.DiscordUser.Username + "#" + trainer.DiscordUser.Discriminator);
            eb.AddField($"Entrenador Perdedor", trainer2.DiscordUser.Username + "#" + trainer2.DiscordUser.Discriminator);
            eb.WithColor(Color.Green);
            return eb.Build();
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
