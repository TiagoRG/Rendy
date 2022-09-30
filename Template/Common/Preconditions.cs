using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Rendy.Common;

// Inherit from PreconditionAttribute
public class RequireRoleAttribute : PreconditionAttribute
{
    // Create a field to store the specified name
    private readonly string _name;

    // Create a constructor so the name can be specified
    public RequireRoleAttribute(string name) => _name = name;

    // Override the CheckPermissions method
    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        // Check if this user is a Guild User, which is the only context where roles exist
        if (context.User is SocketGuildUser gUser)
        {
            var user = await context.Guild.GetUserAsync(context.User.Id) as SocketGuildUser;

            // If this command was executed by a user with the appropriate role, return a success
            if (gUser.Roles.Any(x => x.Name == _name))
                // Since no async work is done, the result has to be wrapped with `Task.FromResult` to avoid compiler errors
                return await Task.FromResult(PreconditionResult.FromSuccess());
            // Since it wasn't, fail
            else
                return await Task.FromResult(PreconditionResult.FromError( $"You must have a role named {context.Guild.Roles.FirstOrDefault(x => x.Name == _name).Mention} to run this command."));
        }
        else
            return await Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
    }
}


/// <summary>
/// Requires the user who executes the command to have MFA enabled.
/// </summary>
public class RequireMFAAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        if ((context.User as SocketSelfUser).IsMfaEnabled)
            return await Task.FromResult(PreconditionResult.FromSuccess());
        else
            return await Task.FromResult(PreconditionResult.FromError("You must have MFA enabled to run this command."));
    }
}

/// <summary>
/// Retains the usage of the command to a list of guilds.
/// </summary>
public class RequireGuildAttribute : PreconditionAttribute
{
    private readonly ulong[] _id;

    public RequireGuildAttribute(ulong[] id) => _id = id;

    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        if (context.Guild is SocketGuild guild)
        {
            if (_id.Any(x => x == guild.Id))
                return await Task.FromResult(PreconditionResult.FromSuccess());
            // Since it wasn't, fail
            else
                return await Task.FromResult(PreconditionResult.FromError($"This command is not available in this guild."));
        }
        else
            return await Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
    }
}