using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices.JavaScript;
using Database;
using Database.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Premium.Api;
using Premium.Logic;
using Premium.Utilities.ExtensionMethods;
using Tools.DateTimeExtension;

namespace Premium.Listeners;

public static class ComponentListener
{
    private static Dictionary<ulong, DateTime> InAction = new();
    
    private static Dictionary<ulong, DateTime> LastNameRestriction = new();

    [AsyncListener(EventTypes.ComponentInteractionCreated)]
    public static async Task ClientOnInteractionReceived(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        if (InAction.TryGetValue(args.User.Id, out var actionDateTime))
        {
            if (actionDateTime > DateTime.UtcNow)
            {
                var interactionResponse = new DiscordInteractionResponseBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Помедленнее!")
                        .WithDescription("⌛ Ваша прошлая интеракция не была завершена!")
                        .WithColor(DiscordColor.Yellow));

                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    interactionResponse.AsEphemeral(true));
                return;
            }

            InAction.Remove(args.User.Id);
        }
        
        var member = await args.Guild.GetMemberAsync(args.User.Id);
        UserProfile? ownerUserProfile = null;

        if (!(args.Id.Contains("role") || args.Id.Contains("buypremium") || args.Id.Contains("purchasepremium") || args.Id.Contains("checkpayment") || args.Id.Contains("cancelpayment")))
        {
            if (member.VoiceState == null)
            {
                var voiceStateNull = new DiscordInteractionResponseBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Управление приватным каналом")
                        .WithDescription($"Вы не можете изменять настройки голосового канала, не находясь в нем!")
                        .WithColor(DiscordColor.Red)
                        .WithThumbnail(args.User.AvatarUrl)
                    );

                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, voiceStateNull.AsEphemeral());
                return;
            }
                    
            if (member.VoiceState.Channel.ParentId != Bot.Config.Channels.PremiumParentId)
            {
                var voiceStateNull = new DiscordInteractionResponseBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Управление приватным каналом")
                        .WithDescription($"Вы не можете изменять настройки голосового канала, который **не является** приватным каналом!")
                        .WithColor(DiscordColor.Red)
                        .WithThumbnail(args.User.AvatarUrl)
                    );

                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, voiceStateNull.AsEphemeral());
                return;
            }

            ownerUserProfile = await MongoManager.GetUserByChannelIdAsync(member.VoiceState.Channel.Id);

            if (ownerUserProfile == null || ownerUserProfile.UserId != args.User.Id /*&& !ownerUserProfile.ModeratorIds.Contains(args.User.Id)*/)
            {
                var leaderRequired = new DiscordInteractionResponseBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Управление приватным каналом")
                        .WithDescription($"Управлять комнатой может **только её лидер**!")
                        .WithColor(DiscordColor.Red)
                        .WithThumbnail(args.User.AvatarUrl)
                    );

                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, leaderRequired.AsEphemeral());
                return;
            }
        }

        InAction.Add(args.User.Id, DateTime.UtcNow.AddSeconds(10));

        Console.WriteLine($"Args id: {args.Id}");
        if (args.Values != null && args.Values?.Length != 0)
        {
            Console.Write("Values: ");

            foreach (var value in args.Values)
            {
                Console.Write($"{value} | ");
                Console.WriteLine();
            }
        }

        var splittedId = args.Id.Split("-");

        try
        {
            switch (args.Id)
            {
                case "purchasepremium":
                {
                    var profile = await MongoManager.GetUserAsync(args.User);
                    
                    if (profile.Payments.CrystalPayment != null && profile.Payments.CrystalPayment.PaymentEndUnixTimestamp > DateTime.UtcNow.ToUnixTimestamp())
                    {
                        var premiumPanel = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Покупка премиум")
                                .WithDescription($"**Вы покупаете:** `{profile.Payments.CrystalPayment.Label}`\n" +
                                                 $"**Стоимость**: `{profile.Payments.CrystalPayment.Amount}₽`")
                                .WithImageUrl(Constants.EmptyLineImageUrl))
                            .AddComponents(new DiscordComponent[]
                            {
                                new DiscordLinkButtonComponent(profile.Payments.CrystalPayment.Url, "Оплатить"),
                                new DiscordButtonComponent(ButtonStyle.Success, "checkpayment", "Проверить оплату"),
                                new DiscordButtonComponent(ButtonStyle.Danger, "cancelpayment", "Отменить"),
                                
                            });

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            premiumPanel.AsEphemeral());
                    }
                    else
                    {
                        string label;
                        int price;
                        var extra = "";
                        
                        switch (args.Values!.First())
                        {
                            case "1":
                            {
                                label = "Premium LvL 1";
                                price = Bot.Config.PremiumPrices.Premium1Price;
                                extra = "premium1";
                                
                                break;
                            }
                            
                            case "2":
                            {
                                label = "Premium LvL 2";
                                price = Bot.Config.PremiumPrices.Premium2Price;
                                extra = "premium2";
                                
                                break;
                            }
                            
                            case "3":
                            {
                                label = "Premium LvL 3";
                                price = Bot.Config.PremiumPrices.Premium3Price;
                                extra = "premium3";
                                
                                break;
                            }
                            
                            default:
                                throw new Exception("Unknown premium was given");
                        }

                        var responseBuilder = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Покупка премиум")
                                .WithDescription($"**Вы собираетесь:** `{label}`\n" +
                                                 $"**Стоимость**: `{price}₽`")
                                .WithImageUrl(Constants.EmptyLineImageUrl)
                            )
                            .AddComponents(new DiscordComponent[]
                            {
                                new DiscordButtonComponent(ButtonStyle.Success, $"buypremium-{args.Values.First()}", "Купить"),
                            });
                        
                        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder.AsEphemeral());
                    }
                    
                    break;
                }

                case var _ when splittedId[0] == "buypremium":
                {
                    string label;
                    int price;
                    var extra = "";
                    
                    switch (splittedId[1])
                    {
                        case "1":
                        {
                            label = "Premium LvL 1";
                            price = Bot.Config.PremiumPrices.Premium1Price;
                            extra = "premium1";
                            
                            break;
                        }
                        
                        case "2":
                        {
                            label = "Premium LvL 2";
                            price = Bot.Config.PremiumPrices.Premium2Price;
                            extra = "premium2";
                            
                            break;
                        }
                        
                        case "3":
                        {
                            label = "Premium LvL 3";
                            price = Bot.Config.PremiumPrices.Premium3Price;
                            extra = "premium3";
                            
                            break;
                        }
                        
                        default:
                            throw new Exception("Unknown premium was given");
                    }
                    
                    var crystalPay = await Bot.CrystalPay.CreateInvoice(price, extra: extra);
                    var userProfile = await MongoManager.GetUserAsync(args.User);
                    userProfile.Payments.CrystalPayment = new PaymentsEntry.CrystalPaymentEntry
                    {
                        Id = crystalPay.Id,
                        Url = crystalPay.Url,
                        Amount = price,
                        Label = label,
                        PaymentEndUnixTimestamp = (DateTime.Now + TimeSpan.FromMinutes(15)).ToUnixTimestamp()
                    };
                    await MongoManager.UpdateAsync(userProfile);
                    
                    var premiumPanel = new DiscordInteractionResponseBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Покупка премиум")
                            .WithDescription($"**Вы покупаете:** `{userProfile.Payments.CrystalPayment.Label}`\n" +
                                             $"**Стоимость**: `{userProfile.Payments.CrystalPayment.Amount}₽`")
                            .WithImageUrl(Constants.EmptyLineImageUrl))
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordLinkButtonComponent(userProfile.Payments.CrystalPayment.Url, "Оплатить"),
                            new DiscordButtonComponent(ButtonStyle.Success, "checkpayment", "Проверить оплату"),
                            new DiscordButtonComponent(ButtonStyle.Danger, "cancelpayment", "Отменить"),
                                
                        });

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                        premiumPanel.AsEphemeral());
                    
                    break;
                }
                
                
                
                case "rename":
                {
                    if (LastNameRestriction.TryGetValue(member.VoiceState.Channel.Id, out var dateTime))
                    {
                        var endRestriction = dateTime.AddMinutes(15);
                        if (endRestriction > DateTime.UtcNow)
                        {
                            var interactionResponse = new DiscordInteractionResponseBuilder()
                                .AddEmbed(new DiscordEmbedBuilder()
                                    .WithTitle("Управление приватным каналом")
                                    .WithDescription("⌛ Вы не можете менять название комнаты так часто!\n" +
                                                     $"Вы можете поменять название комнаты {Formatter.Timestamp(endRestriction)}\n\n" +
                                                     $"__Если вы хотите поменять название комнаты сейчас, вы можете пересоздать комнату__")
                                    .WithColor(DiscordColor.Yellow)
                                    .WithThumbnail(args.User.AvatarUrl));

                            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                interactionResponse.AsEphemeral());
                            break;
                        }

                        LastNameRestriction.Remove(args.User.Id);
                    }

                    var response = new DiscordInteractionResponseBuilder()
                        .WithTitle("Изменение названия")
                        .WithCustomId("renamemodal")
                        .AddComponents(new TextInputComponent("Название:", "name", $"Приват {args.User.Username}",
                            max_length: 100));

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, response);
                    break;
                }

                case "limit":
                {
                    var options = new List<DiscordSelectComponentOption>()
                    {
                        new DiscordSelectComponentOption("Автоматически", "auto",
                            "Ограничить комнату по количеству пользователей в ней"),
                        new DiscordSelectComponentOption("Вручную", "other", "Выбрать своё ограничение комнаты"),
                    };

                    var response = new DiscordInteractionResponseBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Управление приватным каналом")
                            .WithDescription(
                                $"{args.User.Mention}, для установления ограничения канала, **выберите** подходящий способ ограничения в **выпадающем списке ниже**")
                            .WithThumbnail(args.User.AvatarUrl)
                        )
                        .AddComponents(new DiscordSelectComponent("limitdropdown", "Установить лимит пользователей",
                            options));

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        response.AsEphemeral());
                    break;
                }

                case "limitdropdown":
                {
                    switch (args.Values![0])
                    {
                        case "auto":
                        {
                            await member.VoiceState.Channel.ModifyAsync(x =>
                                x.Userlimit = member.VoiceState.Channel.Users.Count);

                            var response = new DiscordInteractionResponseBuilder()
                                .AddEmbed(new DiscordEmbedBuilder()
                                    .WithTitle("Управление приватным каналом")
                                    .WithDescription(
                                        $"{args.User.Mention}, лимит пользователей был успешно обновлен!\n" +
                                        $"Новый лимит пользователей: `{member.VoiceState.Channel.Users.Count}`")
                                    .WithThumbnail(args.User.AvatarUrl)
                                );

                            await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                                response.AsEphemeral());
                            break;
                        }

                        case "other":
                        {
                            var response = new DiscordInteractionResponseBuilder()
                                .WithTitle("Изменение лимита пользователей")
                                .WithCustomId("limitmodal")
                                .AddComponents(new TextInputComponent("Новый лимит:", "limit", "99", max_length: 2));

                            await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, response);
                            break;
                        }
                    }

                    break;
                }

                case "lock":
                {
                    var everyonePermission =
                        member.VoiceState.Channel.PermissionOverwrites.FirstOrDefault(x =>
                            x.Id == args.Interaction.Guild.EveryoneRole.Id);
                    if (everyonePermission == null ||
                        everyonePermission.CheckPermission(Permissions.UseVoice) is PermissionLevel.Allowed
                            or PermissionLevel.Unset)
                    {
                        await member.VoiceState.Channel.AddOverwriteAsync(args.Interaction.Guild.EveryoneRole,
                            deny: Permissions.UseVoice);

                        var response = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление приватным каналом")
                                .WithDescription($"{args.User.Mention}, ваш канал был успешно **закрыт**!\n" +
                                                 $"Теперь в него могут зайти только его **владелец** и **модераторы канала**")
                                .WithThumbnail(args.User.AvatarUrl)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            response.AsEphemeral());
                    }
                    else
                    {
                        await member.VoiceState.Channel.AddOverwriteAsync(args.Interaction.Guild.EveryoneRole,
                            allow: Permissions.UseVoice);

                        var response = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление приватным каналом")
                                .WithDescription($"{args.User.Mention}, ваш канал был успешно **открыт**!\n" +
                                                 $"Теперь в него могут зайти **все**!")
                                .WithThumbnail(args.User.AvatarUrl)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            response.AsEphemeral());
                    }

                    break;
                }

                case "reject":
                {
                    var response = new DiscordInteractionResponseBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Управление приватным каналом")
                            .WithDescription(
                                $"{args.User.Mention}, выберите пользователя из **выпадающего списка ниже**, которого вы **хотите исключить.**")
                            .WithThumbnail(args.User.AvatarUrl)
                        )
                        .AddComponents(new DiscordUserSelectComponent("rejectdropdown", "Выгнать пользователя"));

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        response.AsEphemeral());
                    break;
                }

                case "rejectdropdown":
                {
                    var userId = Convert.ToUInt64(args.Values![0]);

                    DiscordMember targetMember;

                    try
                    {
                        targetMember = await args.Guild.GetMemberAsync(userId);
                    }
                    catch (NotFoundException)
                    {
                        var notFoundResponse = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Приватные каналы")
                                .WithDescription(
                                    $"{args.User.Mention}, выбраннный вами пользователь **не был найден на сервере**.")
                                .WithThumbnail(args.User.AvatarUrl)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                            notFoundResponse);
                        break;
                    }

                    if (member.VoiceState.Channel != targetMember.VoiceState.Channel)
                    {
                        var targetVoiceResponse = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление приватным каналом")
                                .WithDescription(
                                    $"{args.User.Mention}, вы не можете выгнать пользователя, который находится в **другом канале**.")
                                .WithThumbnail(args.User.AvatarUrl)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                            targetVoiceResponse.AsEphemeral());
                        break;
                    }

                    if (ownerUserProfile.UserId == targetMember.Id)
                    {
                        var moderatorTargetResponse = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление приватным каналом")
                                .WithDescription(
                                    $"{args.User.Mention}, вы не можете выгнать пользователя, который является **модератором канала**.")
                                .WithThumbnail(args.User.AvatarUrl)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                            moderatorTargetResponse.AsEphemeral());
                        break;
                    }

                    await member.VoiceState.Channel.AddOverwriteAsync(targetMember, deny: Permissions.UseVoice);
                    await targetMember.ModifyAsync(x => x.VoiceChannel = null);

                    // ownerUserProfile.RejectedUsers.Add(targetMember.Id);
                    // await PrivateVoiceManager.UpdateAsync(ownerUserProfile);

                    var response = new DiscordInteractionResponseBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Управление приватным каналом")
                            .WithDescription(
                                $"{args.User.Mention}, пользователь {targetMember.Mention} был **выгнан и заблокирован в канале**.")
                            .WithThumbnail(args.User.AvatarUrl)
                        );

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                        response.AsEphemeral());
                    break;
                }

                case "invite":
                {
                    var response = new DiscordInteractionResponseBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Управление приватным каналом")
                            .WithDescription(
                                $"{args.User.Mention}, выберите пользователя из **выпадающего списка ниже**, которого вы **пригласить** в этот канал.")
                            .WithThumbnail(args.User.AvatarUrl)
                        )
                        .AddComponents(new DiscordUserSelectComponent("invitedropdown", "Пригласить пользователя"));

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        response.AsEphemeral());
                    break;
                }

                case "invitedropdown":
                {
                    var userId = Convert.ToUInt64(args.Values![0]);
                    DiscordMember targetMember;

                    try
                    {
                        targetMember = await args.Guild.GetMemberAsync(userId);
                    }
                    catch (NotFoundException)
                    {
                        var notFoundResponse = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Приватные каналы")
                                .WithDescription(
                                    $"{args.User.Mention}, выбраннный вами пользователь **не был найден на сервере**.")
                                .WithThumbnail(args.User.AvatarUrl)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                            notFoundResponse);
                        break;
                    }

                    if (targetMember.IsBot)
                    {
                        var botResponse = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Приватные каналы")
                                .WithDescription($"{targetMember.Mention}, вы не можете взаимодействовать с **ботом**.")
                                .WithThumbnail(args.User.AvatarUrl)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, botResponse);
                        break;
                    }

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    await member.VoiceState.Channel.AddOverwriteAsync(targetMember,
                        allow: Permissions.AccessChannels | Permissions.UseVoice);

                    /*if (ownerUserProfile.RejectedUsers.Contains(targetMember.Id))
                {
                    ownerUserProfile.RejectedUsers.Remove(targetMember.Id);
                    await PrivateVoiceManager.UpdateAsync(ownerUserProfile);
                }*/

                    var directMessage = new DiscordMessageBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Приватные каналы")
                            .WithDescription(
                                $"{targetMember.Mention}, пользователь {args.User.Mention} **пригласил вас и выдал доступ** в свой приватный канал {member.VoiceState.Channel.Mention}")
                            .WithThumbnail(args.User.AvatarUrl)
                        )
                        .AddComponents(new DiscordLinkButtonComponent(
                            (await member.VoiceState.Channel.CreateInviteAsync()).ToString(), "Зайти"));

                    try
                    {
                        await targetMember.SendMessageAsync(directMessage);
                    }
                    catch (UnauthorizedException)
                    {
                        // ignored
                    }

                    var response = new DiscordWebhookBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Управление приватным каналом")
                            .WithDescription(
                                $"{args.User.Mention}, пользователь {targetMember.Mention} был **добавлен** и **приглашен** в канал.")
                            .WithThumbnail(args.User.AvatarUrl)
                        );

                    await args.Interaction.EditOriginalResponseAsync(response);
                    break;
                }

                // TODO: доработать как в тз 
                case "voice":
                {
                    var response = new DiscordInteractionResponseBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Управление приватным каналом")
                            .WithDescription(
                                $"{args.User.Mention}, выберите пользователя из **выпадающего списка ниже**, которого вы **хотите замутить/размутить.**")
                            .WithThumbnail(args.User.AvatarUrl)
                        )
                        .AddComponents(new DiscordUserSelectComponent("voicedropdown",
                            "Замутить/Размутить пользователя"));

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        response.AsEphemeral());
                    break;
                }

                case "voicedropdown":
                {
                    var userId = Convert.ToUInt64(args.Values![0]);
                    DiscordMember targetMember;

                    try
                    {
                        targetMember = await args.Guild.GetMemberAsync(userId);
                    }
                    catch (NotFoundException)
                    {
                        var notFoundResponse = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Приватные каналы")
                                .WithDescription(
                                    $"{args.User.Mention}, выбраннный вами пользователь **не был найден на сервере**.")
                                .WithThumbnail(args.User.AvatarUrl)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                            notFoundResponse);
                        break;
                    }

                    if (member.VoiceState.Channel != targetMember.VoiceState.Channel)
                    {
                        var targetVoiceResponse = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление приватным каналом")
                                .WithDescription(
                                    $"{args.User.Mention}, вы не можете замутить пользователя, который находится в **другом канале**.")
                                .WithThumbnail(args.User.AvatarUrl)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                            targetVoiceResponse.AsEphemeral());
                        break;
                    }

                    if (ownerUserProfile.UserId == targetMember.Id)
                    {
                        var moderatorTargetResponse = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление приватным каналом")
                                .WithDescription(
                                    $"{args.User.Mention}, вы не можете замутить пользователя, который модератором канала.")
                                .WithThumbnail(args.User.AvatarUrl)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                            moderatorTargetResponse.AsEphemeral());
                        break;
                    }

                    DiscordInteractionResponseBuilder response;

                    if ((member.VoiceState.Channel.PermissionsFor(targetMember) & Permissions.Speak) != 0)
                    {
                        await member.VoiceState.Channel.AddOverwriteAsync(targetMember, deny: Permissions.Speak);

                        response = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление приватным каналом")
                                .WithDescription(
                                    $"{args.User.Mention}, пользователю {targetMember.Mention} был **выключен** микрофон.")
                                .WithThumbnail(args.User.AvatarUrl)
                            );
                    }
                    else
                    {
                        await member.VoiceState.Channel.AddOverwriteAsync(targetMember, allow: Permissions.Speak);

                        response = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление приватным каналом")
                                .WithDescription(
                                    $"{args.User.Mention}, пользователю {targetMember.Mention} был **включен** микрофон.")
                                .WithThumbnail(args.User.AvatarUrl)
                            );
                    }

                    // reconnect user
                    await targetMember.ModifyAsync(x => x.VoiceChannel = targetMember.VoiceState.Channel);

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                        response.AsEphemeral());
                    break;
                }

                case "hide":
                {
                    var everyonePermission =
                        member.VoiceState.Channel.PermissionOverwrites.FirstOrDefault(x =>
                            x.Id == args.Interaction.Guild.EveryoneRole.Id);
                    if (everyonePermission == null ||
                        everyonePermission.CheckPermission(Permissions.AccessChannels) is PermissionLevel.Allowed
                            or PermissionLevel.Unset)
                    {
                        await member.VoiceState.Channel.AddOverwriteAsync(args.Interaction.Guild.EveryoneRole,
                            deny: Permissions.AccessChannels);

                        var response = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление приватным каналом")
                                .WithDescription($"{args.User.Mention}, ваш канал был успешно **скрыт**!\n" +
                                                 $"Теперь в него могут зайти только его **владелец** и **модераторы канала**")
                                .WithThumbnail(args.User.AvatarUrl)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            response.AsEphemeral());
                    }
                    else
                    {
                        await member.VoiceState.Channel.AddOverwriteAsync(args.Interaction.Guild.EveryoneRole,
                            allow: Permissions.AccessChannels);

                        var response = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление приватным каналом")
                                .WithDescription($"{args.User.Mention}, ваш канал был успешно **открыт**!\n" +
                                                 $"Теперь в него могут зайти **все**!")
                                .WithThumbnail(args.User.AvatarUrl)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            response.AsEphemeral());
                    }

                    break;
                }

                case "muteall":
                {
                    DiscordInteractionResponseBuilder response;

                    if (member.VoiceState.Channel.PermissionOverwrites
                            .FirstOrDefault(x => x.Id == args.Interaction.Guild.EveryoneRole.Id)
                            ?.CheckPermission(Permissions.Speak) is PermissionLevel.Denied)
                    {
                        await member.VoiceState.Channel.AddOverwriteAsync(args.Interaction.Guild.EveryoneRole,
                            allow: Permissions.Speak);

                        response = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление приватным каналом")
                                .WithDescription(
                                    $"{args.User.Mention}, в вашем канале был успешно **включен микрофон**!\n" +
                                    $"Теперь в нем могут говорить **все**!")
                                .WithThumbnail(args.User.AvatarUrl)
                            );
                    }
                    else
                    {
                        await member.VoiceState.Channel.AddOverwriteAsync(args.Interaction.Guild.EveryoneRole,
                            deny: Permissions.Speak);

                        response = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление приватным каналом")
                                .WithDescription(
                                    $"{args.User.Mention}, в вашем канале был успешно **выключен микрофон**!\n" +
                                    $"Теперь в нем могут говорить только его **владелец** и **модераторы канала**")
                                .WithThumbnail(args.User.AvatarUrl)
                            );
                    }

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        response.AsEphemeral());

                    // reconnect all users
                    foreach (var discordMember in member.VoiceState.Channel.Users)
                    {
                        try
                        {
                            await discordMember.ModifyAsync(x => x.VoiceChannel = discordMember.VoiceState.Channel);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    break;
                }

                case "reset":
                {
                    var response = new DiscordInteractionResponseBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Управление приватным каналом")
                            .WithDescription(
                                $"{args.User.Mention}, выберите пользователя из **выпадающего списка ниже**, которому вы **сбросить права.**")
                            .WithThumbnail(args.User.AvatarUrl)
                        )
                        .AddComponents(new DiscordUserSelectComponent("resetdropdown", "Сбросить права пользователя"));

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        response.AsEphemeral());
                    break;
                }

                case "resetdropdown":
                {
                    var userId = Convert.ToUInt64(args.Values![0]);

                    DiscordMember targetMember;

                    try
                    {
                        targetMember = await args.Guild.GetMemberAsync(userId);
                    }
                    catch (NotFoundException)
                    {
                        var notFoundResponse = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Приватные каналы")
                                .WithDescription(
                                    $"{args.User.Mention}, выбраннный вами пользователь **не был найден на сервере**.")
                                .WithThumbnail(args.User.AvatarUrl)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                            notFoundResponse);
                        break;
                    }

                    await member.VoiceState.Channel.AddOverwriteAsync(targetMember);

                    // ownerUserProfile.RejectedUsers.Add(targetMember.Id);
                    // await PrivateVoiceManager.UpdateAsync(ownerUserProfile);

                    var response = new DiscordInteractionResponseBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Управление приватным каналом")
                            .WithDescription(
                                $"{args.User.Mention}, права пользователя {targetMember.Mention} были **обнулены в канале**.")
                            .WithThumbnail(args.User.AvatarUrl)
                        );

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                        response.AsEphemeral());
                    break;
                }

                case var _ when splittedId[0] == "changerole":
                {
                    /*var userId = Convert.ToUInt64(splittedId.Last());

                    if (userId != args.User.Id)
                    {
                        var notUserResponse = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithDescription("Вы не можете использовать интеракции другого пользоавтеля.")
                                .WithColor(DiscordColor.Red)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            notUserResponse.AsEphemeral());
                        break;
                    }*/

                    var userProfile = await MongoManager.GetUserAsync(args.User);

                    if (userProfile.Premium.PremiumLevel != PremiumEntry.PremiumLevelEntry.Premium3)
                    {
                        var noDonateResponse = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление личной ролью")
                                .WithDescription($"**Ваш уровень премиума не позволяет использовать личную роль**")
                                .WithColor(DiscordColor.Red)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, noDonateResponse.AsEphemeral());
                        break;
                    }

                    if (userProfile.Premium.EndPremiumDateUnix <= DateTime.Now.ToUnixTimestamp())
                    {
                        var noDonateResponse = new DiscordInteractionResponseBuilder()
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Управление личной ролью")
                                .WithDescription($"Ваш премиум закончился")
                                .WithColor(DiscordColor.Red)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, noDonateResponse.AsEphemeral());
                        break;
                    }
                    
                    if (userProfile.Premium.CustomRole.RoleId == null || !await RolesMethods.CheckRoleExistsAsync((ulong)userProfile.Premium.CustomRole.RoleId))
                    {
                        var noRoleMessage = new DiscordInteractionResponseBuilder()
                            .WithContent(args.User.Mention)
                            .AddEmbed(new DiscordEmbedBuilder()
                                .WithTitle("Ошибка")
                                .WithDescription("Ваша роль не была найдена.")
                                .WithColor(DiscordColor.Red)
                            );

                        await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, noRoleMessage.AsEphemeral());
                        break;
                    }

                    if (userProfile.Premium.CustomRole.RoleId == null) throw new Exception("Custom role id can't be null");
                    var role = args.Guild.GetRole((ulong)userProfile.Premium.CustomRole.RoleId);

                    var nameRole = $"{args.User.Username} role";
                    if (nameRole.Length > 30) nameRole = "role name";
                    
                    var response = new DiscordInteractionResponseBuilder()
                        .WithTitle("Изменение роли")
                        .WithCustomId("changerolemodal")
                        .AddComponents(new TextInputComponent("Название роли:", "rolename", nameRole, $"{role.Name}", max_length: 30))
                        .AddComponents(new TextInputComponent("HEX цвет:", "hexcolor", "#FF0000", $"{role.Color}", max_length: 7));

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, response);
                    break;
                }
            }
        }
        finally
        {
            InAction.Remove(args.User.Id);
        }
    }
    
    [AsyncListener(EventTypes.ModalSubmitted)]
    public static async Task ClientOnModalSubmitted(DiscordClient client, ModalSubmitEventArgs args)
    {
        Console.WriteLine($"Modal args id: {args.Interaction.Data.CustomId}");
        if (args.Values != null && args.Values.Count != 0)
        {
            Console.Write("Values: ");
            
            foreach (var value in args.Values)
            {
                Console.Write($"{value} | ");
                Console.WriteLine();
            }
        }

        var member = await args.Interaction.Guild.GetMemberAsync(args.Interaction.User.Id);

        if (args.Interaction.Data.CustomId != "changerolemodal")
        {
            if (member.VoiceState == null)
            {
                var voiceStateNull = new DiscordInteractionResponseBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Управление приватным каналом")
                        .WithDescription($"Вы не можете изменять настройки голосового канала, не находясь в нем!")
                        .WithColor(DiscordColor.Red)
                        .WithThumbnail(args.Interaction.User.AvatarUrl)
                    );

                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, voiceStateNull.AsEphemeral());
                return;
            }
                
            if (member.VoiceState.Channel.ParentId != Bot.Config.Channels.PremiumParentId)
            {
                var voiceStateNull = new DiscordInteractionResponseBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Управление приватным каналом")
                        .WithDescription($"Вы не можете изменять настройки голосового канала, который **не является** приватным каналом!")
                        .WithColor(DiscordColor.Red)
                        .WithThumbnail(args.Interaction.User.AvatarUrl)
                    );

                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, voiceStateNull.AsEphemeral());
                return;
            }
        
            var ownerUserProfile = await MongoManager.GetUserByChannelIdAsync(member.VoiceState.Channel.Id);

            if (ownerUserProfile == null || ownerUserProfile.UserId != args.Interaction.User.Id)
            {
                var leaderRuquired = new DiscordInteractionResponseBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Управление приватным каналом")
                        .WithDescription($"Управлять комнатой может **только её лидер**!")
                        .WithColor(DiscordColor.Red)
                        .WithThumbnail(args.Interaction.User.AvatarUrl)
                    );

                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, leaderRuquired.AsEphemeral());
                return;
            }
        }

        switch (args.Interaction.Data.CustomId)
        {
            case "renamemodal":
            {
                var newName = args.Values!.First(x => x.Key == "name").Value;
                await member.VoiceState.Channel.ModifyAsync(x => x.Name = newName);
                LastNameRestriction.Add(member.VoiceState.Channel.Id, DateTime.UtcNow);

                var response = new DiscordInteractionResponseBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Управление приватным каналом")
                        .WithDescription($"{args.Interaction.User.Mention}, канал был успешно обнолвен!")
                        .WithColor(DiscordColor.Green)
                        .WithThumbnail(args.Interaction.User.AvatarUrl)
                    );

                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    response.AsEphemeral());
                break;
            }

            case "limitmodal":
            {
                var limit = Convert.ToInt32(args.Values!.First(x => x.Key == "limit").Value);
                await member.VoiceState.Channel.ModifyAsync(x => x.Userlimit = limit);
                
                var response = new DiscordInteractionResponseBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Управление приватным каналом")
                        .WithDescription($"{args.Interaction.User.Mention}, лимит пользователей был успешно обновлен!\n" +
                                         $"Новый лимит пользователей: `{limit}`")
                        .WithColor(DiscordColor.Green)
                        .WithThumbnail(args.Interaction.User.AvatarUrl)
                    );

                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response.AsEphemeral());
                break;
            }
            
            case "changerolemodal":
            {
                var roleName = args.Values.First(x => x.Key == "rolename").Value;
                var hexColor = args.Values.First(x => x.Key == "hexcolor").Value;

                var userProfile = await MongoManager.GetUserAsync(args.Interaction.User);
                
                if (userProfile.Premium.CustomRole.RoleId == null || !await RolesMethods.CheckRoleExistsAsync((ulong)userProfile.Premium.CustomRole.RoleId ))
                {
                    var noRoleMessage = new DiscordInteractionResponseBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithTitle("Ошибка")
                            .WithDescription("Ваша роль не была найдена, попробуйте заново вызвать команду и создать её.")
                            .WithColor(DiscordColor.Red)
                        );

                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, noRoleMessage.AsEphemeral());
                    break;
                }
                
                await args.Interaction.DeferAsync(true);

                var dcolor = ColorMethods.ParseColorOrNull(hexColor) ?? new DiscordColor("#000000");

                var role = args.Interaction.Guild.GetRole((ulong)userProfile.Premium.CustomRole.RoleId );
                await role.ModifyAsync(name: roleName, color: dcolor);
                await RolesMethods.MoveRoleTopAsync(role);
                
                var reponse = new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Успешно!")
                        .WithDescription("Роль была успешно изменена:\n" +
                                         $"**Название:** `{role.Name}`\n" +
                                         $"**Цвет:** `{role.Color.ToString()}`")
                        .WithColor(Constants.MainColor)
                        .WithImageUrl(Constants.EmptyLineImageUrl)
                    );

                await args.Interaction.EditOriginalResponseAsync(reponse);
                break;
            }
        }
    }
}