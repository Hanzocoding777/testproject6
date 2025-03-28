using Database.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Premium.Logic;

namespace Premium.Listeners;

public class StatusListener
{
    // [AsyncListener(EventTypes.PresenceUpdated)]
    public static async Task ClientOnMessageCreated(DiscordClient client, PresenceUpdateEventArgs args)
    {
        var userProfile = await MongoManager.GetUserAsync(args.User);
        var newStreamActivity = args.PresenceAfter?.Activities.FirstOrDefault(act => act.ActivityType == ActivityType.Streaming);
        if (userProfile.Premium.Active && newStreamActivity != null)
        {
            var member = await Bot.Guild.GetMemberAsync(args.User.Id);
            var liveStreamRole = Bot.Guild.GetRole(Bot.Config.Roles.LiveStreamRoleId);
            await member.GrantRoleAsync(liveStreamRole);
        }
        else if (userProfile.Premium.Active && newStreamActivity == null)
        {
            var member = await Bot.Guild.GetMemberAsync(args.User.Id);
            if (member.Roles.Any(x => x.Id == Bot.Config.Roles.LiveStreamRoleId))
            {
                var liveStreamRole = Bot.Guild.GetRole(Bot.Config.Roles.LiveStreamRoleId);
                await member.RevokeRoleAsync(liveStreamRole);
            }
        }
    }
}