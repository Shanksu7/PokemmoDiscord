using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AKDiscordBot.Extensions
{
    class RequireAdminIdAttribute : PreconditionAttribute
    {
        public RequireAdminIdAttribute()
        {

        }
        
        public static List<ulong> AllowedIDS = new List<ulong>()
        {
            //any admin id
            207520984591368202
        };

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (!AllowedIDS.Contains(context.User.Id))
                return Task.FromResult(PreconditionResult.FromError("No administrator permission"));
            else
                return Task.FromResult(PreconditionResult.FromSuccess());
        }


    }
}
