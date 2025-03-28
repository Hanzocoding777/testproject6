using Database.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Premium.Logic;
using Premium.Utilities.ExtensionMethods;
using Tools.DateTimeExtension;

namespace Premium.Listeners;

public class GuildMemberListener
{
    [AsyncListener(EventTypes.GuildMemberRemoved)]
    public static async Task ClientOnGuildMemberRemoved(DiscordClient client, GuildMemberRemoveEventArgs args)
    {
        var profile = await MongoManager.GetUserAsync(args.Member);

        if (profile.Premium.Active && profile.Premium.EndPremiumDateUnix > DateTime.Now.ToUnixTimestamp())
        {
            if (profile.Premium.CustomRole.RoleId != null)
            {
                try
                {
                    var role = Bot.Guild.GetRole((ulong)profile.Premium.CustomRole.RoleId);
                    if (role != null) await role.DeleteAsync("Удаление старой личной роли");
                }
                catch (Exception)
                {
                   // ignore
                }
            }

            if (profile.Premium.CustomChannel.ChannelId != null)
            {
                try
                {
                    var channel = Bot.Guild.GetChannel((ulong)profile.Premium.CustomChannel.ChannelId);
                    if (channel != null) await channel.DeleteAsync("Удаление личного канала");
                }
                catch (Exception)
                {
                    // ignore
                }
            }
            
            profile.Premium.Active = false;
            profile.Premium.Nitro = false;
            profile.Premium.IsNotificated = false;
            profile.Premium.BoostCount = 0;
            profile.Premium.CustomRole.RoleId = null;
            profile.Premium.CustomChannel.ChannelId = null;
            await MongoManager.UpdateAsync(profile);
        }
    }
}