using Discord;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PokemmoDiscord.PokemonBot.Attributes;
using PokemmoDiscord.PokemonBot.Data;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AKDiscordBot
{
    class Program
    {
        public static string pre = ".";

        //Property to access Bot's information client
        public static DiscordSocketClient Client { get; set; }
        //Property for commands
        public static CommandService Commands { get; set; }
        //Property for Services
        private IServiceProvider Services { get; set; }
        private static string bot_Token = Environment.GetEnvironmentVariable("bot_pokemon");
        private static ulong logChannel = 542133670602080258;
        //METHODS
        //Main Method
        static void Main(string[] args) => new Program().RuntBotAsync().GetAwaiter().GetResult();

        public async Task RuntBotAsync()
        {
            Client = new DiscordSocketClient();
            Commands = new CommandService();
            Services = new ServiceCollection().AddSingleton(Client).AddSingleton(Commands).BuildServiceProvider();
            Client.Log += Log;
            Client.JoinedGuild += OnJoinedGuild;
            Client.LeftGuild += OnLeftGuild;
            Client.UserBanned += OnUserBanned;
            Client.RoleCreated += OnRoleCreated;
            Client.Ready += OnReady;
            //Client.UserJoined += OnUserJoined;
            await RegisterCommandsAsync();
            await Client.LoginAsync(TokenType.Bot, bot_Token);
            await Client.StartAsync();
            await Task.Delay(-1);
            
        }


        private async Task OnReady()
        {
            await PokemonData.LoadData();
            await ChannelPokemon.StartSpawn();
            await PokemonData.SaveData();
            foreach (var guild in Client.Guilds)
            {
                if (guild.Id != 542108567487119372)
                {
                    await guild.LeaveAsync();
                }
            }
        }

        public static async Task SetGameAsync(string activity)
        {
            Console.WriteLine(activity);
            await Client.SetGameAsync(activity);
        }
        private async Task OnRoleCreated(SocketRole arg)
        {
            if (arg.Name == Client.CurrentUser.Username)
            {
                await SendReport("Role created **" + arg.Name + "**\nAdmin?: **" + arg.Permissions.Administrator + "**\nGuild: **" + arg.Guild.Name + "** (" + arg.Id + ")", ReportEnum.LOG);
            }
        }

        private async Task OnUserBanned(SocketUser arg1, SocketGuild arg2)
        {
            if (arg2.Id != 264445053596991498)
            {
                await SendReport("**" + arg1.Username + "#" + arg1.Discriminator + "** (" + arg1.Id + ") \n **" + arg2.Name + "** (" + arg2.Id + ")", ReportEnum.USER_BANNED_FROM_GUILD);
            }
        }

        private async Task OnLeftGuild(SocketGuild arg)
        {
            await SendReport($"I've LEFT from guild **{arg.Name}** ({arg.Id})", ReportEnum.GUILD_LEFT);
        }

        private async Task OnJoinedGuild(SocketGuild arg)
        {
            await SendReport($"I've JOINED to guild **{arg.Name}** ({arg.Id})", ReportEnum.GUILD_JOIN);
        }

        private async Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            await Task.CompletedTask;
        }

        //MessageReceived is the function that runs when Bot receive a Message on a channel or DM
        public async Task RegisterCommandsAsync()
        {
            Client.MessageReceived += HandleCommandAsync;
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message is null || message.Author.IsBot)            
                return;            
            var trainer = PokemonData._trainers.GetOrAdd(message.Author.Id, new PokemmoDiscord.PokemonBot.Entity.Trainer(message.Author.Id));
            if (trainer.InFight && message.Content.Length == 1)
            {                
                await trainer.Fight.SetMovement(trainer.ID, int.Parse(message.Content) - 1);
                await message.DeleteAsync();
                return;
            }
            int argPos = 0;
            var cmdexecuted = false;
            if (message.HasStringPrefix(pre, ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(Client, message);
                var result = await Commands.ExecuteAsync(context, argPos, Services);
                cmdexecuted = true;
                if (result.Error.HasValue && ChannelPokemon.ChannelsToSpawn.Any(x => x.ChId == message.Channel.Id))
                    await message.DeleteAsync();
            }
            if(!cmdexecuted && ChannelPokemon.ChannelsToSpawn.Any(x => x.ChId == message.Channel.Id))
                await message.DeleteAsync();
            
        }

        public static async Task SendReport(string msg, ReportEnum type)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            var log = Client.GetChannel(logChannel) as SocketTextChannel;

            switch (type)
            {
                case ReportEnum.ERROR:
                    embedBuilder.WithColor(Color.Orange);
                    break;
                case ReportEnum.GUILD_JOIN:
                    embedBuilder.WithColor(Color.Green);
                    break;
                case ReportEnum.GUILD_LEFT:
                    embedBuilder.WithColor(Color.Red);
                    break;
                case ReportEnum.KONNONEWS:
                    embedBuilder.WithColor(Color.Blue);
                    break;
                case ReportEnum.USER_BANNED_FROM_GUILD:
                    embedBuilder.WithColor(Color.Red);
                    break;
                case ReportEnum.LOG:
                    embedBuilder.WithColor(Color.DarkGreen);
                    break;
            }
            embedBuilder.WithTitle(type.ToString());
            embedBuilder.WithDescription(msg);
            var embed = embedBuilder.Build();
            Console.WriteLine(msg);
            await log.SendMessageAsync(embed: embed);
        }
        public enum ReportEnum
        {
            GUILD_JOIN,
            GUILD_LEFT,
            USER_BANNED_FROM_GUILD,
            KONNONEWS,
            ERROR,
            LOG,
        }
    }
}
