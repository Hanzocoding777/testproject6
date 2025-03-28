using System.Diagnostics.CodeAnalysis;
using Database.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Premium.Logic;
using Timer = System.Timers.Timer; 

namespace Premium.Listeners;

public static class VoiceStateListener
{
    public static Dictionary<ulong, DateTime> LastChannelCreate = new();
    
    [AsyncListener(EventTypes.VoiceStateUpdated)]
    public static async Task ClientOnVoiceStateUpdate(DiscordClient client, VoiceStateUpdateEventArgs args)
    {
        if (args.Before?.Channel == args.After?.Channel)
        {
            return;
        }

        // Auto room creation
        switch (args.After?.Channel)
        {
            case null:
                break;

            case var channel when channel.Id == Bot.Config.Channels.CreatePremiumChannelId:
                {
                    var member = await args.Guild.GetMemberAsync(args.User.Id);

                    if (LastChannelCreate.TryGetValue(args.User.Id, out var dateTime))
                    {
                        if (dateTime.AddSeconds(10) > DateTime.UtcNow)
                        {
                            var timeOutBuilder = new DiscordMessageBuilder()
                                .AddEmbed(new DiscordEmbedBuilder()
                                    .WithTitle("⏳ Остыньте!")
                                    .WithDescription("Вы пытаетесь создать игровой канал слишком часто!\n" +
                                                     $"Вам необходимо подождать ещё **{(dateTime.AddSeconds(10) - DateTime.UtcNow).Seconds} сек.** перед созданием нового канала.")
                                    .WithColor(DiscordColor.Yellow)
                                );

                            try
                            {
                                await member.SendMessageAsync(timeOutBuilder);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }

                            await member.ModifyAsync(x => x.VoiceChannel = null);
                            break;
                        }
                        else
                        {
                            LastChannelCreate.Remove(member.Id);
                        }
                    }
                    
                    var userProfile = await MongoManager.GetUserAsync(member);

                    if (userProfile.Premium.CustomChannel.ChannelId == null)
                    {
                        var gameChannel = await args.Guild.CreateChannelAsync(userProfile.Premium.CustomChannel.Name, ChannelType.Voice, parent: args.Guild.GetChannel(Bot.Config.Channels.PremiumParentId), position: 0);
                        await gameChannel.AddOverwriteAsync(member, allow: Permissions.AccessChannels | Permissions.Stream | Permissions.UseVoice | Permissions.PrioritySpeaker | Permissions.UseVoiceDetection | Permissions.Speak);
                        
                        LastChannelCreate.Add(member.Id, DateTime.UtcNow);

                        try
                        {
                            await member.ModifyAsync(x => x.VoiceChannel = gameChannel);
                            userProfile.Premium.CustomChannel.ChannelId = gameChannel.Id;
                        }
                        catch (Exception)
                        {
                            userProfile.Premium.CustomChannel.ChannelId = null;
                            await gameChannel.DeleteAsync();
                        }
                    }
                    else
                    {
                        var privateChannel = args.Guild.GetChannel((ulong)userProfile.Premium.CustomChannel.ChannelId);
                        if (privateChannel == null)
                        {
                            var gameChannel = await args.Guild.CreateChannelAsync(userProfile.Premium.CustomChannel.Name, ChannelType.Voice, parent: args.Guild.GetChannel(Bot.Config.Channels.PremiumParentId), position: 0);
                            await gameChannel.AddOverwriteAsync(member, allow: Permissions.AccessChannels | Permissions.Stream | Permissions.UseVoice | Permissions.PrioritySpeaker | Permissions.UseVoiceDetection | Permissions.Speak);
                            
                            LastChannelCreate.Add(member.Id, DateTime.UtcNow);

                            try
                            {
                                await member.ModifyAsync(x => x.VoiceChannel = gameChannel);
                                userProfile.Premium.CustomChannel.ChannelId = gameChannel.Id;
                            }
                            catch (Exception)
                            {
                                userProfile.Premium.CustomChannel.ChannelId = null;
                                await gameChannel.DeleteAsync();
                            }
                        }
                        else
                        {
                            await member.ModifyAsync(x => x.VoiceChannel = privateChannel);
                        }
                    }
                    
                    
                    await MongoManager.UpdateAsync(userProfile);
                    
                    break;
                }
        }


        // Invitation delete or edit
        switch (args.Before?.Channel)
        {
            case null:
                break;

            // Invitation creation or editing
            case var channel when channel.Parent.Id == Bot.Config.Channels.PremiumParentId:
            {
                int searchForMembersCount = channel.UserLimit ?? 0 - channel.Users.Count(x => !x.IsBot);
                
                if (!channel.Users.Any(x => x.IsBot == false) || searchForMembersCount <= 0)
                {
                    var timer = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
                    timer.Start();
                    timer.Elapsed += async (sender, eventArgs) =>
                    {
                        var regetChannel = args.Guild.GetChannel(channel.Id);
                        if (regetChannel != null)
                        {
                            searchForMembersCount = regetChannel.UserLimit ?? 0 - regetChannel.Users.Count(x => !x.IsBot);
                        
                            if (!regetChannel.Users.Any(x => x.IsBot == false) || searchForMembersCount <= 0)
                            {
                                try
                                {
                                    await channel.DeleteAsync();
                                }
                                catch (Exception)
                                {
                                    // ignore
                                }
                        
                                var userProfile = await MongoManager.GetUserByChannelIdAsync(channel.Id);
                                if (userProfile != null)
                                {
                                    userProfile.Premium.CustomChannel.ChannelId = null;
                                    await MongoManager.UpdateAsync(userProfile);
                                }
                            }
                        }
                        
                        timer.Stop();
                        timer.Dispose();
                    };
                }
                
                break;
            }
        }
    }
}