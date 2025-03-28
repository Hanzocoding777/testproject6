using System.Timers;
using Database.Services;
using DSharpPlus.Entities;
using Premium.Utilities.ExtensionMethods;
using Tools.DateTimeExtension;
using Timers = System.Timers.Timer;

namespace Premium.Listeners;

public class Timers
{
    public static async Task RegisterTimers()
    {
        var checkOutdatetDonatersTimer = new System.Timers.Timer(TimeSpan.FromMinutes(15).TotalMilliseconds);
        checkOutdatetDonatersTimer.Elapsed += CheckOutdatetDonatersAsync;
        checkOutdatetDonatersTimer.AutoReset = true;
        checkOutdatetDonatersTimer.Enabled = true;
        
        var checkNitroDonatersTimer = new System.Timers.Timer(TimeSpan.FromMinutes(15).TotalMilliseconds);
        checkNitroDonatersTimer.Elapsed += CheckNitroDonatersAsync;
        checkNitroDonatersTimer.AutoReset = true;
        checkNitroDonatersTimer.Enabled = true;

        await Task.CompletedTask;
    }
    
    private static async void CheckOutdatetDonatersAsync(object? source, ElapsedEventArgs args)
    {
        var donatorProfiles = (await MongoManager.GetAllPremiumUsersAsync()).ToList();

        var nearOutDatingProfiles = donatorProfiles.Where(x => DateTime.Now >= x.Premium.EndPremiumDateUnix.ToDateTime() - TimeSpan.FromDays(5) && !x.Premium.IsNotificated).ToList();

        foreach (var nearOutDatingProfile in nearOutDatingProfiles)
        {
            try
            {
                var member = await Bot.Guild.GetMemberAsync(nearOutDatingProfile.UserId);

                var notificationMessage = new DiscordMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Уведмоление о премиуме")
                        .WithThumbnail(member.AvatarUrl)
                        .WithDescription($"{member.Mention}, ваш премиум **скоро заканчивается**.\n\n" +
                                         $"Чтобы не потерять его, вам необходимо продлить её в канале {Bot.Guild.GetChannel(Bot.Config.Channels.PremiumPanelChannelId).Mention}")
                        .WithColor(Constants.MainColor)
                        .WithFooter($"С уважением, администрация {Bot.Guild.Name}", Bot.Guild.IconUrl)
                    );

                await member.SendMessageAsync(notificationMessage);
            }
            catch (Exception)
            {
                // ignored
            }

            nearOutDatingProfile.Premium.IsNotificated = true;
            await MongoManager.UpdateAsync(nearOutDatingProfile);
        }
        
        // профиля которые уже просрочились
        var outDatedProfiles = donatorProfiles.Where(x => DateTime.Now >= x.Premium.EndPremiumDateUnix.ToDateTime() && x.Premium.Active).ToList();
        foreach (var outDatedProfile in outDatedProfiles)
        {
            try
            {
                var member = await Bot.Guild.GetMemberAsync(outDatedProfile.UserId);
                await PremiumMethods.RevokePremiumRolesAsync(member);

                var notificationMessage = new DiscordMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Уведомоление о премиуме")
                        .WithThumbnail(member.AvatarUrl)
                        .WithDescription($"**Ваш премиум закончился**")
                        .WithColor(Constants.MainColor)
                        .WithFooter($"С уважением, администрация {Bot.Guild.Name}", Bot.Guild.IconUrl)
                    );

                await member.SendMessageAsync(notificationMessage);
            }
            catch (Exception)
            {
                // ignored
            }

            if (outDatedProfile.Premium.CustomRole.RoleId != null)
            {
                try
                {
                    var role = Bot.Guild.GetRole((ulong)outDatedProfile.Premium.CustomRole.RoleId);
                    if (role != null) await role.DeleteAsync("Удаление старой личной роли");
                }
                catch (Exception)
                {
                   // ignore
                }
            }

            if (outDatedProfile.Premium.CustomChannel.ChannelId != null)
            {
                try
                {
                    var channel = Bot.Guild.GetChannel((ulong)outDatedProfile.Premium.CustomChannel.ChannelId);
                    if (channel != null) await channel.DeleteAsync("Удаление личного канала");
                }
                catch (Exception)
                {
                    // ignore
                }
            }
            
            outDatedProfile.Premium.Active = false;
            outDatedProfile.Premium.Nitro = false;
            outDatedProfile.Premium.IsNotificated = false;
            outDatedProfile.Premium.BoostCount = 0;
            outDatedProfile.Premium.CustomRole.RoleId = null;
            outDatedProfile.Premium.CustomChannel.ChannelId = null;
            await MongoManager.UpdateAsync(outDatedProfile);

            var user = await Bot.Client.GetUserAsync(outDatedProfile.UserId);
            await PremiumMethods.LogEndPremiumAsync(user, outDatedProfile);
        }
    }

    private static async void CheckNitroDonatersAsync(object? source, ElapsedEventArgs args)
    {
        var premiumProfiles = (await MongoManager.GetAllNitroPremiumUsersAsync()).ToList();

        foreach (var premiumProfile in premiumProfiles)
        {
            var member = await Bot.Guild.GetMemberAsync(premiumProfile.UserId);
            if (member.PremiumSince == null)
            {
                try
                {
                    await PremiumMethods.RevokePremiumRolesAsync(member);

                    var notificationMessage = new DiscordMessageBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Уведомоление о премиуме")
                            .WithThumbnail(member.AvatarUrl)
                            .WithDescription($"**Ваш премиум был снят в связи с отсутсвием нитро бустов**")
                            .WithColor(Constants.MainColor)
                            .WithFooter($"С уважением, администрация {Bot.Guild.Name}", Bot.Guild.IconUrl)
                        );

                    await member.SendMessageAsync(notificationMessage);
                }
                catch (Exception)
                {
                    // ignored
                }

                if (premiumProfile.Premium.CustomRole.RoleId != null)
                {
                    try
                    {
                        var role = Bot.Guild.GetRole((ulong)premiumProfile.Premium.CustomRole.RoleId);
                        if (role != null) await role.DeleteAsync("Удаление старой личной роли");
                    }
                    catch (Exception)
                    {
                       // ignore
                    }
                }

                if (premiumProfile.Premium.CustomChannel.ChannelId != null)
                {
                    try
                    {
                        var channel = Bot.Guild.GetChannel((ulong)premiumProfile.Premium.CustomChannel.ChannelId);
                        if (channel != null) await channel.DeleteAsync("Удаление личного канала");
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }
                
                premiumProfile.Premium.Active = false;
                premiumProfile.Premium.Nitro = false;
                premiumProfile.Premium.IsNotificated = false;
                premiumProfile.Premium.BoostCount = 0;
                premiumProfile.Premium.CustomRole.RoleId = null;
                premiumProfile.Premium.CustomChannel.ChannelId = null;
                await MongoManager.UpdateAsync(premiumProfile);
                
                var user = await Bot.Client.GetUserAsync(premiumProfile.UserId);
                await PremiumMethods.LogEndPremiumAsync(user, premiumProfile);
            }
        }
    }
}