using System.Drawing;
using Database;
using Database.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Premium.Utilities.ExtensionMethods;
using Tools.DateTimeExtension;

namespace Premium.SlashCommands;

[GuildOnly]
public class PremiumCommands : ApplicationCommandModule
{
    [SlashCommand("премиум-магазин", "Отправить сообщение премиум магазина")]
    public async Task ShopMessage(InteractionContext ctx)
    {
        var options = new List<DiscordSelectComponentOption>()
        {
            new DiscordSelectComponentOption(
                "Premium LvL 1",
                "1",
                emoji: new DiscordComponentEmoji(Constants.Add)),

            new DiscordSelectComponentOption(
                "Premium LvL 2",
                "2",
                emoji: new DiscordComponentEmoji(Constants.Add)),
            
            new DiscordSelectComponentOption(
                "Premium LvL 3",
                "3",
                emoji: new DiscordComponentEmoji(Constants.Add)),
        };
        
        var shopMessageBuilder = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithImageUrl(Constants.EmptyLineImageUrl)
                .WithColor(Constants.InvisibleColor)
            )
            .AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("Магазин")
                .WithDescription("Premium\nБазовая роль Premium lv1\nдаёт следующие преимущества:\n\n\u25ab\ufe0f Уникальный цвет\n\u25ab\ufe0f Отображение в топе над списком игроков справа\n\u25ab\ufe0f Возможность использовать сторонние эмоджи и стикеры\n\u25ab\ufe0f Уменьшение времени задержки обновления объявлений в канале поиска (15 вмеcто 30 секунд )\n\u25ab\ufe0f Отдельный чат premium для пользователей\n\u25ab\ufe0f Объявление в поиске команды будет отображаться вверху списка и выделяться  отдельным цветом\n\nPremium lv2\nВместе с Premium+ Вы получите:\n\n\u25ab\ufe0f Все возможности Premium\n\u25ab\ufe0f Новый цвет премиум-роли\n\u25ab\ufe0f Подключение к заполненным голосовым (паблик) каналам\n\u25ab\ufe0f Перемещение пользователей к себе в войс из паблик каналов\n\u25ab\ufe0f Роль Live Stream во время онлайн трансляций на Twitch\n\u25ab\ufe0f Отдельный чат premium для пользователей\n\u25ab\ufe0f Личная комната с полным удобным управлением:\n\u2800\u2800\u2800 -\u2800Изменение названия и лимита\n\u2800\u2800\u2800 -\u2800Возможность кикать/банить людей в своей комнате\n\u2800\u2800\u2800 -\u2800Скрытие комнаты от других пользователей\n\u2800\u2800\u2800 -\u2800Выдать доступ пользователю к вашей комнате.\n\nPremium lv3\nВместе с ролью Premium++ Вы получите:\n\n\u25ab\ufe0f Все возможности Premium\n\u25ab\ufe0f Все возможности Premium+\n\u25ab\ufe0f Новый цвет премиум-роли\n\u25ab\ufe0f Своя кастомная роль (название и цвет)\n\u25ab\ufe0f Подключение к заполненным голосовым (паблик и ранкед) каналам\n\u25ab\ufe0f Роль Live Stream во время онлайн трансляций на Twitch\n\u25ab\ufe0f Отдельный чат premium для пользователей\n\u25ab\ufe0f Возможность скрыть роли статистики (adr/kda")
            )
            .AddEmbed(new DiscordEmbedBuilder()
                .WithDescription("**Нажмите** на панель ниже, чтобы **выбрать** необходимый уровень премиума.")
            )
            .AddComponents(new DiscordSelectComponent("purchasepremium", "Выберите нужный премиум", options));

        await ctx.Channel.SendMessageAsync(shopMessageBuilder);
        await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().WithContent("Успешно отправлено")
            .AsEphemeral());
    }
    
    [SlashCommand("выдать-премиум", "Выдать | Продлить премиум")]
    public async Task GrantPremium(InteractionContext ctx, [Option("Пользователь", "Премиум пользователь")] DiscordUser discordUser,
        [Option("Срок", "Срок выдачи | продления премиума")] string timeString,
        [Option("Премиум", "Уровень премиума")] PremiumEntry.PremiumLevelEntry? premiumLevel)
    {
        await ctx.DeferAsync();
        
        var userProfile = await MongoManager.GetUserAsync(discordUser);
        var timeSpan = TimeSpanMethods.TimeSpanParse(timeString);

        if (premiumLevel != null)
        {
            userProfile.Premium.PremiumLevel = (PremiumEntry.PremiumLevelEntry)premiumLevel;
        }
        
        var member = await ctx.Guild.GetMemberAsync(discordUser.Id);

        if (member == null)
        {
            var memberMessage = new DiscordInteractionResponseBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithDescription($"🔴 Пользователь не был найден на сервере")
                    .WithColor(Constants.InvisibleColor)
                    .WithFooter(Bot.Guild.Name, Bot.Guild.IconUrl)
                );

            await ctx.CreateResponseAsync(memberMessage);
            
            return;
        }

        if (userProfile.Premium.PremiumLevel == PremiumEntry.PremiumLevelEntry.Premium3)
        {
            var customRole = await ctx.Guild.CreateRoleAsync(userProfile.Premium.CustomRole.Name,
                color: new DiscordColor(userProfile.Premium.CustomRole.HexColor));
            await RolesMethods.MoveRoleTopAsync(customRole);

            await member.GrantRoleAsync(customRole);
            
            userProfile.Premium.CustomRole.RoleId = customRole.Id;
        }
        
        
        userProfile.Premium.Active = true;
        userProfile.Premium.EndPremiumDateUnix += (ulong)timeSpan.TotalSeconds;
        await MongoManager.UpdateAsync(userProfile);

        try
        {
            var memberMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithDescription($"Ваш уровень Premium обновлен\n\n" +
                                     $"**Текущий уровень:** {userProfile.Premium.PremiumLevel.GetName()} \n" +
                                     $"**Заканчивается:** {Formatter.Timestamp(userProfile.Premium.EndPremiumDateUnix.ToDateTime(), TimestampFormat.LongDateTime)}")
                    .WithColor(Constants.InvisibleColor)
                    .WithFooter(Bot.Guild.Name, Bot.Guild.IconUrl)
                );

            await PremiumMethods.GrantPremiumRolesAsync(member, userProfile);
            
            await member.SendMessageAsync(memberMessage);
        }
        catch (Exception)
        {
            var userErrorMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithDescription($"⚠ Сообщение пользователю {discordUser.Mention} **не было доставлено**")
                    .WithColor(Constants.InvisibleColor)
                );

            await ctx.Channel.SendMessageAsync(userErrorMessage);
        }
        
        var logMessage = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithDescription($"Администратор {ctx.User.Mention} **выдал | продлил** премиум\n" +
                                 $"**Текущий уровень:** {userProfile.Premium.PremiumLevel.GetName()}\n" +
                                 $"**Пользователь:** {discordUser.Mention}\n" +
                                 $"**Заканчивается:** {Formatter.Timestamp(userProfile.Premium.EndPremiumDateUnix.ToDateTime(), TimestampFormat.LongDateTime)}")
                .WithColor(Constants.InvisibleColor)
            );
        
        var logChannel = ctx.Guild.GetChannel(Bot.Config.Channels.PremiumLogChannelId);
        await logChannel.SendMessageAsync(logMessage);
        
        var response = new DiscordWebhookBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithAuthor(discordUser.Username, iconUrl: discordUser.AvatarUrl)
                .WithDescription($"**Уровень премиума:** {userProfile.Premium.PremiumLevel.GetName()}\n" +
                                 $"**Заканчивается:** {Formatter.Timestamp(userProfile.Premium.EndPremiumDateUnix.ToDateTime(), TimestampFormat.LongDateTime)}")
                .WithColor(Constants.InvisibleColor)
                .WithThumbnail(discordUser.AvatarUrl)
                .WithImageUrl(Constants.EmptyLineImageUrl)
            );

        await ctx.EditResponseAsync(response);
    }

    [SlashCommand("снять-премиум", "снять премиум")]
    public async Task GrantPremium(InteractionContext ctx,
        [Option("Пользователь", "Премиум пользователь")] DiscordUser discordUser)
    {
        await ctx.DeferAsync();
        
        var userProfile = await MongoManager.GetUserAsync(discordUser);
        
        var logMessage = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithDescription($"Администратор {ctx.User.Mention} **снял** премиум\n" +
                                 $"**Текущий уровень:** {userProfile.Premium.PremiumLevel.GetName()}\n" +
                                 $"**Пользователь:** {discordUser.Mention}\n" +
                                 $"**Заканчивается:** {Formatter.Timestamp(userProfile.Premium.EndPremiumDateUnix.ToDateTime(), TimestampFormat.LongDateTime)}")
                .WithColor(Constants.InvisibleColor)
            );
        
        var logChannel = ctx.Guild.GetChannel(Bot.Config.Channels.PremiumLogChannelId);
        await logChannel.SendMessageAsync(logMessage);

        userProfile.Premium.Active = false;
        userProfile.Premium.EndPremiumDateUnix = 0;
        userProfile.Premium.PremiumLevel = PremiumEntry.PremiumLevelEntry.Premium1;

        try
        {
            var member = await ctx.Guild.GetMemberAsync(discordUser.Id);
            await PremiumMethods.RevokePremiumRolesAsync(member);
        }
        catch (Exception)
        {
            // ignored
        }

        if (userProfile.Premium.CustomChannel.ChannelId != null)
        {
            var customChannel = ctx.Guild.GetChannel((ulong)userProfile.Premium.CustomChannel.ChannelId);
            if (customChannel != null)
            {
                try
                {
                    await customChannel.DeleteAsync();
                }
                catch (Exception)
                {
                    // pass
                }
            }
            
            userProfile.Premium.CustomChannel.ChannelId = null;

        }

        if (userProfile.Premium.CustomRole.RoleId != null)
        {
            var customRole = ctx.Guild.GetRole((ulong)userProfile.Premium.CustomRole.RoleId!);
            if (customRole != null)
            {
                try
                {
                    await customRole.DeleteAsync();
                }
                catch (Exception)
                {
                    // pass
                }
            }
            
            userProfile.Premium.CustomRole.RoleId = null;
        }

        await MongoManager.UpdateAsync(userProfile);
        
        var response = new DiscordWebhookBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithAuthor(discordUser.Username, iconUrl: discordUser.AvatarUrl)
                .WithDescription($"Пользователь: {discordUser.Mention}\n\n" +
                                 $"Премиум пользователя был снят")
                .WithColor(Constants.InvisibleColor)
                .WithThumbnail(discordUser.AvatarUrl)
                .WithImageUrl(Constants.EmptyLineImageUrl)
            );

        await ctx.EditResponseAsync(response);
    }

    [SlashCommand("инфо-премиум", "Посмотреть информацию о премиуме")]
    public async Task InfoPremium(InteractionContext ctx,
        [Option("Пользователь", "Премиум пользователь")] DiscordUser discordUser)
    {
        var userProfile = await MongoManager.GetUserAsync(discordUser);

        var response = new DiscordInteractionResponseBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithAuthor(discordUser.Username, iconUrl: discordUser.AvatarUrl)
                .WithDescription($"**Статус:** `{(userProfile.Premium.Active ? "🟢" : "🔴")}`\n" +
                                 $"**Заканчивается:** {Formatter.Timestamp(userProfile.Premium.EndPremiumDateUnix.ToDateTime(), TimestampFormat.LongDateTime)}\n\n" +
                                 $"Кастомный канал: `{(userProfile.Premium.CustomChannel.ChannelId != null ? $"<#{userProfile.Premium.CustomChannel.ChannelId}>" : "Отсутсвует")}`\n\n" +
                                 $"Личная роль: `{(userProfile.Premium.CustomRole.RoleId != null ? $"<@&{userProfile.Premium.CustomRole.RoleId}>" : "Отсутсвует")}`")
                .WithColor(Constants.InvisibleColor)
                .WithImageUrl(Constants.EmptyLineImageUrl)
            );

        await ctx.CreateResponseAsync(response);
    }

    [SlashCommand("премиум-сообщение", "Отправить сообщение о премиуме")]
    public async Task PremiumMessage(InteractionContext ctx)
    {
        await ctx.DeferAsync(true);
        
        var prem1Message = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("Premium lvl 1")
                .WithDescription("\u25ab\ufe0f Уникальный цвет\n\u25ab\ufe0f Отображение в топе над списком игроков справа\n\u25ab\ufe0f Возможность использовать сторонние эмоджи и стикеры\n\u25ab\ufe0f Уменьшение времени задержки обновления объявлений в канале поиска (15 вмеcто 30 секунд )\n\u25ab\ufe0f Отдельный чат premium для пользователей\n\u25ab\ufe0f Объявление в поиске команды будет отображаться вверху списка и выделяться  отдельным цветом")
                .WithColor(Constants.MainColor)
            )
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Success, "buypremium-1", "Купить премиум lvl 1", true)
            });

        await ctx.Channel.SendMessageAsync(prem1Message);
        
        var prem2Message = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("Premium lvl 2")
                .WithDescription("\u25ab\ufe0f Все возможности Premium\n\u25ab\ufe0f Новый цвет премиум-роли\n\u25ab\ufe0f Подключение к заполненным голосовым (паблик) каналам\n\u25ab\ufe0f Перемещение пользователей к себе в войс из паблик каналов\n\u25ab\ufe0f Роль Live Stream во время онлайн трансляций на Twitch\n\u25ab\ufe0f Отдельный чат premium для пользователей\n\u25ab\ufe0f Личная комната с полным удобным управлением:\n\u2800\u2800\u2800 -\u2800Изменение названия и лимита\n\u2800\u2800\u2800 -\u2800Возможность кикать/банить людей в своей комнате\n\u2800\u2800\u2800 -\u2800Скрытие комнаты от других пользователей\n\u2800\u2800\u2800 -\u2800Выдать доступ пользователю к вашей комнате.")
                .WithColor(Constants.MainColor)
            )
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Success, "buypremium-2", "Купить премиум lvl 2", true)
            });

        await ctx.Channel.SendMessageAsync(prem2Message);
        
        var prem3Message = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("Premium lvl 3")
                .WithDescription("\u25ab\ufe0f Все возможности Premium\n\u25ab\ufe0f Все возможности Premium+\n\u25ab\ufe0f Новый цвет премиум-роли\n\u25ab\ufe0f Своя кастомная роль (название и цвет)\n\u25ab\ufe0f Подключение к заполненным голосовым (паблик и ранкед) каналам\n\u25ab\ufe0f Роль Live Stream во время онлайн трансляций на Twitch\n\u25ab\ufe0f Отдельный чат premium для пользователей\n\u25ab\ufe0f Возможность скрыть роли статистики (adr/kda")
                .WithColor(Constants.MainColor)
            )
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Success, "buypremium-3", "Купить премиум lvl 3", true)
            });

        await ctx.Channel.SendMessageAsync(prem3Message);
        

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("✅ Успешно отправлено"));
    }

    [SlashCommand("премиум-панель", "Отправить панель использования премиума")]
    public async Task PremiumPanel(InteractionContext ctx)
    {
        // Первое сообщение - только с изображением (будет отображаться над вторым сообщением)
        var imageMessage = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithImageUrl(Constants.EmptyLineImageUrl)
                .WithColor(Constants.InvisibleColor)
            );
        var message = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
            .WithTitle("Premium меню")
            .WithDescription(
                "> **Для использования функций премиум меню пользуйтесь кнопками под этим сообщением!**\n\n" +
                "```ㅤㅤㅤㅤㅤУправление кастомной комнатой```\n" +
                $"{Constants.Pencil} -  Изменить название вашей кастомной комнаты \n\n" +
                $"{Constants.Add} - Установить лимит пользователей \n\n" +
                $"{Constants.Lock} - Закрыть комнату \n\n" +
                $"{Constants.RejectUser} - Запретить доступ пользователю \n\n" +
                $"{Constants.InviteUser} - Выдать доступ пользователю \n\n" +
                $"{Constants.Microphone} - Настроить право использования микрофона \n\n" +
                $"{Constants.HideEye} - Скрыть комнату \n\n" +
                $"{Constants.Mute} - Забрать право говорить у всех \n\n" +
                "```ㅤㅤㅤㅤУправление личной ролью```\n" +
                $"{Constants.Paint} - Изменить личную роль")
            )
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "rename", Constants.EmptyString, emoji: new DiscordComponentEmoji(Constants.Pencil)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "limit", Constants.EmptyString, emoji: new DiscordComponentEmoji(Constants.Add)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "lock", Constants.EmptyString, emoji: new DiscordComponentEmoji(Constants.Lock)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "reject", Constants.EmptyString, emoji: new DiscordComponentEmoji(Constants.RejectUser)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "invite", Constants.EmptyString, emoji: new DiscordComponentEmoji(Constants.InviteUser)),  
            })
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "1", Constants.EmptyString, disabled: true),
                new DiscordButtonComponent(ButtonStyle.Secondary, "voice", Constants.EmptyString, emoji: new DiscordComponentEmoji(Constants.Microphone)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "hide", Constants.EmptyString, emoji: new DiscordComponentEmoji(Constants.HideEye)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "muteall", Constants.EmptyString, emoji: new DiscordComponentEmoji(Constants.Mute)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "2", Constants.EmptyString, disabled: true),
                // new DiscordButtonComponent(ButtonStyle.Secondary, "reset", Constants.EmptyString, emoji: new DiscordComponentEmoji(Constants.Lock)),
            })
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "3", Constants.EmptyString, disabled: true),
                new DiscordButtonComponent(ButtonStyle.Secondary, "4", Constants.EmptyString, disabled: true),
                new DiscordButtonComponent(ButtonStyle.Secondary, "changerole", Constants.EmptyString, emoji: new DiscordComponentEmoji(Constants.Paint)),
                new DiscordButtonComponent(ButtonStyle.Secondary, "5", Constants.EmptyString, disabled: true),
                new DiscordButtonComponent(ButtonStyle.Secondary, "6", Constants.EmptyString, disabled: true),
            });

        try
        {
            await ctx.Channel.SendMessageAsync(imageMessage);
            await ctx.Channel.SendMessageAsync(message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        await ctx.CreateResponseAsync("сообщение было успешно было отправлено", true);
    }
}