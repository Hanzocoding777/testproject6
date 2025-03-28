using System.Runtime.InteropServices.JavaScript;
using Database;
using Database.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Premium.Logic;
using Premium.Utilities.ExtensionMethods;
using Tools.DateTimeExtension;

namespace Premium.Listeners;

public class MessageListener
{
    [AsyncListener(EventTypes.MessageCreated)]
    public static async Task ClientOnMessageCreated(DiscordClient client, MessageCreateEventArgs args)
    {
        if (args.Message.MessageType == MessageType.UserPremiumGuildSubscription)
        {
            var userProfile = await MongoManager.GetUserAsync(args.Author);

            if (userProfile.Premium.BoostCount >= 2)
                userProfile.Premium.BoostCount = 0;
            
            userProfile.Premium.BoostCount += 1;
            
            if (userProfile.Premium.BoostCount == 1)
            {
                await MongoManager.UpdateAsync(userProfile);
                
                var moreBoostMessageBuilder = new DiscordMessageBuilder()
                    .WithAllowedMention(UserMention.All)
                    .WithContent(args.Author.Mention)
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithDescription(">>> Спасибо за **буст** нашего сервера!\n" +
                                         $"Вам осталось **совсем чуть-чуть до награды**\n\n" +
                                         $"Забустите наш сервер **ещё раз** и вы получите премиум 1 уровня!")
                        .WithTimestamp(DateTime.Now)
                        .WithColor(Constants.InvisibleColor)
                    );
                
                var mainChannel = args.Guild.GetChannel(Bot.Config.Channels.MainChatChannelId);
                await mainChannel.SendMessageAsync(moreBoostMessageBuilder);
                return;
            }
            
            if (userProfile.Premium.BoostCount == 2)
            {
                userProfile.Premium.EndPremiumDateUnix += (DateTime.Now + TimeSpan.FromDays(31)).ToUnixTimestamp();
                userProfile.Premium.Active = true;
                userProfile.Premium.Nitro = true;
                userProfile.Premium.PremiumLevel = PremiumEntry.PremiumLevelEntry.Premium1;

                var member = await args.Guild.GetMemberAsync(args.Author.Id);
            
                await PremiumMethods.GrantPremiumRolesAsync(member, userProfile);
            
                var customRole = await args.Guild.CreateRoleAsync(userProfile.Premium.CustomRole.Name,
                    color: new DiscordColor(userProfile.Premium.CustomRole.HexColor));
                await RolesMethods.MoveRoleTopAsync(customRole);

                userProfile.Premium.CustomRole.RoleId = customRole.Id;
                await MongoManager.UpdateAsync(userProfile);
            
                var message = new DiscordMessageBuilder()
                    .WithAllowedMention(UserMention.All)
                    .WithContent(args.Author.Mention)
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithDescription(">>> Спасибо за ** два буста** нашего сервера!\n" +
                                         $"Вам выдан **премиум первого уровня на месяц!**")
                        .WithTimestamp(DateTime.Now)
                        .WithColor(Constants.InvisibleColor)
                    );
        
                var mainChat = args.Guild.GetChannel(Bot.Config.Channels.MainChatChannelId);
                await mainChat.SendMessageAsync(message);
            }
        }
    }
}