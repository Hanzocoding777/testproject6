using Database;
using DSharpPlus.Entities;
using Serilog;

namespace Premium.Utilities.ExtensionMethods;

public class RolesMethods
{
    private static readonly ILogger Logger = Log.ForContext<RolesMethods>();
    
    public static async Task<bool> CheckRoleExistsAsync(ulong roleId)
    {
        var guild = await Bot.Client.GetGuildAsync(Bot.Config.GuildId);

        bool succes = false;

        try
        {
            guild.GetRole(roleId);
            succes = true;
        }
        catch (Exception e)
        {
            Logger.Information($"Role {roleId} was not found");
        }

        return succes;
    }

    public static async Task MoveRoleTopAsync(DiscordRole role)
    {
        var guild = await Bot.Client.GetGuildAsync(Bot.Config.GuildId);
        var dRole = guild.GetRole(Bot.Config.Roles.HighestRoleId);
        await role.ModifyPositionAsync(dRole.Position-1, reason: "update donate role");
    }
}