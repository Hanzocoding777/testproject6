using System.Drawing;
using Database;
using Database.Services;
using DSharpPlus.Entities;
using Tools.DateTimeExtension;

namespace Premium.Utilities.ExtensionMethods;

public static class PremiumMethods
{
    public static async Task LogStartPremiumAsync(DiscordUser premiumMember, UserProfile userProfile, DiscordMember? grantMember = null)
    {
        var logMessageBuilder = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("Премиум")
                .WithDescription($"`Выдал:` {(grantMember == null ? "Автоматически" : grantMember.Mention)}\n" +
                                 $"`Пользователь:` {premiumMember.Mention}\n" +
                                 $"`За буст:` {(userProfile.Premium.Nitro ? "Да" : "Нет")}" +
                                 $"`Выдано на:` {(userProfile.Premium.EndPremiumDateUnix.ToDateTime() - DateTime.Now).Days}")
                .WithColor(DiscordColor.Yellow)
            );

        var logChannel = Bot.Guild.GetChannel(Bot.Config.Channels.PremiumPanelChannelId);
        await logChannel.SendMessageAsync(logMessageBuilder);
    }
    
    public static async Task LogEndPremiumAsync(DiscordUser premiumMember, UserProfile userProfile, DiscordMember? grantMember = null)
    {
        var logMessageBuilder = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("[ Премиум закончился ]")
                .WithDescription($"`Пользователь:` {premiumMember.Mention}\n" +
                                 $"`За буст:` {(userProfile.Premium.EndPremiumDateUnix.ToDateTime() - DateTime.Now).Days}")
                .WithColor(DiscordColor.Red)
            );

        var logChannel = Bot.Guild.GetChannel(Bot.Config.Channels.PremiumPanelChannelId);
        await logChannel.SendMessageAsync(logMessageBuilder);
    }
    
    public static async Task GrantPremiumRolesAsync(DiscordMember member, UserProfile userProfile)
    {
        switch (userProfile.Premium.PremiumLevel)
        {
            case PremiumEntry.PremiumLevelEntry.Premium1:
                var premium1 = member.Guild.GetRole(Bot.Config.Roles.Premium1RoleId);
                await member.GrantRoleAsync(premium1);
                break;
            case PremiumEntry.PremiumLevelEntry.Premium2:
                var premium2 = member.Guild.GetRole(Bot.Config.Roles.Premium2RoleId);
                await member.GrantRoleAsync(premium2);
                break;
            case PremiumEntry.PremiumLevelEntry.Premium3:
                var premium3 = member.Guild.GetRole(Bot.Config.Roles.Premium3RoleId);
                await member.GrantRoleAsync(premium3);
                break;
            default:
                throw new ArgumentOutOfRangeException("Unknown premium level");
        }
    }

    public static async Task RevokePremiumRolesAsync(DiscordMember member)
    {
        if (member.Roles.Any(x => x.Id == Bot.Config.Roles.Premium1RoleId))
        {
            var premium1 = member.Guild.GetRole(Bot.Config.Roles.Premium1RoleId);
            await member.RevokeRoleAsync(premium1);
        }

        if (member.Roles.Any(x => x.Id == Bot.Config.Roles.Premium2RoleId))
        {
            var premium2 = member.Guild.GetRole(Bot.Config.Roles.Premium2RoleId);
            await member.RevokeRoleAsync(premium2);
        }

        if (member.Roles.Any(x => x.Id == Bot.Config.Roles.Premium3RoleId))
        {
            var premium3 = member.Guild.GetRole(Bot.Config.Roles.Premium3RoleId);
            await member.RevokeRoleAsync(premium3);
        }
    }
}