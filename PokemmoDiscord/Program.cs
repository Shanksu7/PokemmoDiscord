using System;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PokemmoDiscord.PokemonBot.Data;

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
        private static string bot_Token = "NTQxNzI3ODYxMTc4ODkyMjg4.Dzpktg.EStExITh9cx2kRV8eNJRKCwD8eE";
        private static ulong logChannel = 542133670602080258;
        //METHODS
        //Main Method
        static void Main(string[] args) => new Program().RuntBotAsync().GetAwaiter().GetResult();
        
        public async Task RuntBotAsync()
        {
            //await PokemonData.LoadData();
            Client = new DiscordSocketClient();
            Commands = new CommandService();
            Services = new ServiceCollection().AddSingleton(Client).AddSingleton(Commands).BuildServiceProvider();
            //event subscriiptions
            Client.Log += Log;
            Client.JoinedGuild += OnJoinedGuild;
            Client.LeftGuild += OnLeftGuild;        
            Client.UserBanned += OnUserBanned;
            Client.RoleCreated += OnRoleCreated;
            Client.Ready += OnReady;
            Client.UserJoined += OnUserJoined;
            //await Client.SetGameAsync(pre+"help Rayearth (= `w´=)7 ");
            await RegisterCommandsAsync();            
            //             
            await Client.LoginAsync(TokenType.Bot, bot_Token);
            await Client.StartAsync();            
            await Task.Delay(-1);
        }

        private async Task OnUserJoined(SocketGuildUser arg)
        {
            //kanto role
            //await arg.AddRoleAsync(arg.Guild.GetRole(542129659513405445));
        }

        private async Task OnReady()
        {
            await PokemonData.LoadData();
            await ChannelPokemon.Start();
            foreach (var guild in Client.Guilds)
                if (guild.Id != 542108567487119372)
                    await guild.LeaveAsync();
        }

        public static async Task SetGameAsync(string activity)
        {
            Console.WriteLine(activity);
            await Client.SetGameAsync(activity);
        }
        private async Task OnRoleCreated(SocketRole arg)
        {
            if(arg.Name == Client.CurrentUser.Username)
                await SendReport("Role created **"+ arg.Name + "**\nAdmin?: **" + arg.Permissions.Administrator+ "**\nGuild: **" +arg.Guild.Name+ "** ("+arg.Id+")", ReportEnum.LOG);
        }

        private async Task OnUserBanned(SocketUser arg1, SocketGuild arg2)
        {
            if(arg2.Id != 264445053596991498)
                await SendReport("**"+arg1.Username + "#" + arg1.Discriminator + "** (" + arg1.Id + ") \n **" + arg2.Name + "** (" + arg2.Id + ")", ReportEnum.USER_BANNED_FROM_GUILD);
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
            //await SendReport(arg.ToString(), ReportEnum.LOG);                        
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

            int argPos = 0;
            if (message.HasStringPrefix(pre, ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {

                //This block ocurrs when the message start with Bot's prefix or @mention 
                var context = new SocketCommandContext(Client, message);
                var result = await Commands.ExecuteAsync(context, argPos, Services);

            }
        }
        
        public static async Task SendReport(string msg, ReportEnum type)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();            
            var log = Client.GetChannel(logChannel) as SocketTextChannel;

            switch(type)
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
