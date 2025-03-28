using DSharpPlus;
using DSharpPlus.SlashCommands;
using Serilog;
using System.Reflection;
using Premium.Entities;

namespace Premium.Logic;

[AttributeUsage(AttributeTargets.Method)]
public class AsyncListenerAttribute : Attribute
{
    public EventTypes Target { get; }

    public AsyncListenerAttribute(EventTypes targetType)
    {
        Target = targetType;
    }

    public void Register(Bot bot, DiscordClient client, MethodInfo listener)
    {
        Task OnEventWithArgs(DiscordClient client, object e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await ((Task)listener.Invoke(null, new[] { client, e })!)!;
                }
                catch (Exception ex)
                {
                    var logger = Log.ForContext<AsyncListenerAttribute>();

                    await ErrorMessageSender.SendError(ex);

                    logger.Error("{exception}", $"Uncaught exception in listener thread: \n{ex}");
                    logger.Error("{exception}", ex.StackTrace);
                }
            });
            return Task.CompletedTask;
        }

        Task OnScommandsEvent(SlashCommandsExtension cmd, object e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await ((Task)listener.Invoke(null, new[] { cmd, e })!)!;
                }
                catch (Exception ex)
                {
                    var logger = Log.ForContext<AsyncListenerAttribute>();

                    await ErrorMessageSender.SendError(ex);

                    logger.Error("{exception}", $"<AsyncListener> Uncaught exception in listener thread: \n{ex}");
                    logger.Error("{exception}", ex.StackTrace);
                }
            });
            return Task.CompletedTask;
        }

        switch (Target)
        {
            case EventTypes.AutocompleteErrored:
                Bot.SlashCommands.AutocompleteErrored += OnScommandsEvent;
                break;

            case EventTypes.AutocompleteExecuted:
                Bot.SlashCommands.AutocompleteExecuted += OnScommandsEvent; 
                break;

            case EventTypes.ChannelCreated:
                client.ChannelCreated += OnEventWithArgs;
                break;

            case EventTypes.ChannelDeleted:
                client.ChannelDeleted += OnEventWithArgs;
                break;

            case EventTypes.ChannelPinsUpdated:
                client.ChannelPinsUpdated += OnEventWithArgs;
                break;

            case EventTypes.ChannelUpdated:
                client.ChannelUpdated += OnEventWithArgs;
                break;

            case EventTypes.ClientErrored:
                client.ClientErrored += OnEventWithArgs;
                break;

            case EventTypes.SlashCommandErrored:
                Bot.SlashCommands.SlashCommandErrored += OnScommandsEvent;
                break;

            case EventTypes.SlashCommandExecuted:
                Bot.SlashCommands.SlashCommandExecuted += OnScommandsEvent;
                break;

            case EventTypes.ComponentInteractionCreated:
                client.ComponentInteractionCreated += OnEventWithArgs;
                break;

            case EventTypes.ContextMenuInteractionCreated:
                client.ContextMenuInteractionCreated += OnEventWithArgs;
                break;

            case EventTypes.ContextMenuExecuted:
                Bot.SlashCommands.ContextMenuExecuted += OnScommandsEvent;
                break;

            case EventTypes.ContextMenuErrored:
                Bot.SlashCommands.ContextMenuErrored += OnScommandsEvent;
                break;

            case EventTypes.ContextMenuInvoked:
                Bot.SlashCommands.ContextMenuInvoked += OnScommandsEvent;
                break;

            case EventTypes.DmChannelDeleted:
                client.DmChannelDeleted += OnEventWithArgs;
                break;

            case EventTypes.GuildAvailable:
                client.GuildAvailable += OnEventWithArgs;
                break;

            case EventTypes.GuildBanAdded:
                client.GuildBanAdded += OnEventWithArgs;
                break;

            case EventTypes.GuildBanRemoved:
                client.GuildBanRemoved += OnEventWithArgs;
                break;

            case EventTypes.GuildCreated:
                client.GuildCreated += OnEventWithArgs;
                break;

            case EventTypes.GuildDeleted:
                client.GuildDeleted += OnEventWithArgs;
                break;

            case EventTypes.GuildDownloadCompleted:
                client.GuildDownloadCompleted += OnEventWithArgs;
                break;

            case EventTypes.GuildEmojisUpdated:
                client.GuildEmojisUpdated += OnEventWithArgs;
                break;

            case EventTypes.GuildIntegrationsUpdated:
                client.GuildIntegrationsUpdated += OnEventWithArgs;
                break;

            case EventTypes.GuildMemberAdded:
                client.GuildMemberAdded += OnEventWithArgs;
                break;

            case EventTypes.GuildMemberRemoved:
                client.GuildMemberRemoved += OnEventWithArgs;
                break;

            case EventTypes.GuildMembersChunked:
                client.GuildMembersChunked += OnEventWithArgs;
                break;

            case EventTypes.GuildMemberUpdated:
                client.GuildMemberUpdated += OnEventWithArgs;
                break;

            case EventTypes.GuildRoleCreated:
                client.GuildRoleCreated += OnEventWithArgs;
                break;

            case EventTypes.GuildRoleDeleted:
                client.GuildRoleDeleted += OnEventWithArgs;
                break;

            case EventTypes.GuildRoleUpdated:
                client.GuildRoleUpdated += OnEventWithArgs;
                break;

            case EventTypes.GuildStickersUpdated:
                client.GuildStickersUpdated += OnEventWithArgs;
                break;

            case EventTypes.GuildUnavailable:
                client.GuildUnavailable += OnEventWithArgs;
                break;

            case EventTypes.GuildUpdated:
                client.GuildUpdated += OnEventWithArgs;
                break;

            case EventTypes.Heartbeated:
                client.Heartbeated += OnEventWithArgs;
                break;

            case EventTypes.IntegrationCreated:
                client.IntegrationCreated += OnEventWithArgs;
                break;

            case EventTypes.IntegrationDeleted:
                client.IntegrationDeleted += OnEventWithArgs;
                break;

            case EventTypes.IntegrationUpdated:
                client.IntegrationUpdated += OnEventWithArgs;
                break;

            case EventTypes.InteractionCreated:
                client.InteractionCreated += OnEventWithArgs;
                break;

            case EventTypes.InviteCreated:
                client.InviteCreated += OnEventWithArgs;
                break;

            case EventTypes.InviteDeleted:
                client.InviteDeleted += OnEventWithArgs;
                break;

            case EventTypes.MessageAcknowledged:
                client.MessageAcknowledged += OnEventWithArgs;
                break;

            case EventTypes.MessageCreated:
                client.MessageCreated += OnEventWithArgs;
                break;

            case EventTypes.MessageDeleted:
                client.MessageDeleted += OnEventWithArgs;
                break;

            case EventTypes.MessageReactionAdded:
                client.MessageReactionAdded += OnEventWithArgs;
                break;

            case EventTypes.MessageReactionRemoved:
                client.MessageReactionRemoved += OnEventWithArgs;
                break;

            case EventTypes.MessageReactionRemovedEmoji:
                client.MessageReactionRemovedEmoji += OnEventWithArgs;
                break;

            case EventTypes.MessageReactionsCleared:
                client.MessageReactionsCleared += OnEventWithArgs;
                break;

            case EventTypes.MessagesBulkDeleted:
                client.MessagesBulkDeleted += OnEventWithArgs;
                break;

            case EventTypes.MessageUpdated:
                client.MessageUpdated += OnEventWithArgs;
                break;

            case EventTypes.ModalSubmitted:
                client.ModalSubmitted += OnEventWithArgs;
                break;

            case EventTypes.PresenceUpdated:
                client.PresenceUpdated += OnEventWithArgs;
                break;

            case EventTypes.Ready:
                client.Ready += OnEventWithArgs;
                break;

            case EventTypes.Resumed:
                client.Resumed += OnEventWithArgs;
                break;

            case EventTypes.ScheduledGuildEventCompleted:
                client.ScheduledGuildEventCompleted += OnEventWithArgs;
                break;

            case EventTypes.ScheduledGuildEventCreated:
                client.ScheduledGuildEventCreated += OnEventWithArgs;
                break;

            case EventTypes.ScheduledGuildEventDeleted:
                client.ScheduledGuildEventDeleted += OnEventWithArgs;
                break;

            case EventTypes.ScheduledGuildEventUpdated:
                client.ScheduledGuildEventUpdated += OnEventWithArgs;
                break;

            case EventTypes.ScheduledGuildEventUserAdded:
                client.ScheduledGuildEventUserAdded += OnEventWithArgs;
                break;

            case EventTypes.ScheduledGuildEventUserRemoved:
                client.ScheduledGuildEventUserRemoved += OnEventWithArgs;
                break;

            case EventTypes.SocketClosed:
                client.ScheduledGuildEventUserRemoved += OnEventWithArgs;
                break;

            case EventTypes.SocketErrored:
                client.SocketClosed += OnEventWithArgs;
                break;

            case EventTypes.SocketOpened:
                client.SocketOpened += OnEventWithArgs;
                break;

            case EventTypes.StageInstanceCreated:
                client.StageInstanceCreated += OnEventWithArgs;
                break;

            case EventTypes.StageInstanceDeleted:
                client.StageInstanceDeleted += OnEventWithArgs;
                break;

            case EventTypes.StageInstanceUpdated:
                client.StageInstanceUpdated += OnEventWithArgs;
                break;

            case EventTypes.ThreadCreated:
                client.ThreadCreated += OnEventWithArgs;
                break;

            case EventTypes.ThreadDeleted:
                client.ThreadDeleted += OnEventWithArgs;
                break;

            case EventTypes.ThreadListSynced:
                client.ThreadListSynced += OnEventWithArgs;
                break;

            case EventTypes.ThreadMembersUpdated:
                client.ThreadMembersUpdated += OnEventWithArgs;
                break;

            case EventTypes.ThreadMemberUpdated:
                client.ThreadMemberUpdated += OnEventWithArgs;
                break;

            case EventTypes.ThreadUpdated:
                client.ThreadUpdated += OnEventWithArgs;
                break;

            case EventTypes.TypingStarted:
                client.TypingStarted += OnEventWithArgs;
                break;

            case EventTypes.UnknownEvent:
                client.UnknownEvent += OnEventWithArgs;
                break;

            case EventTypes.UserSettingsUpdated:
                client.UserSettingsUpdated += OnEventWithArgs;
                break;

            case EventTypes.UserUpdated:
                client.UserUpdated += OnEventWithArgs;
                break;

            case EventTypes.VoiceServerUpdated:
                client.VoiceServerUpdated += OnEventWithArgs;
                break;

            case EventTypes.VoiceStateUpdated:
                client.VoiceStateUpdated += OnEventWithArgs;
                break;

            case EventTypes.WebhooksUpdated:
                client.WebhooksUpdated += OnEventWithArgs;
                break;

            case EventTypes.Zombied:
                client.Zombied += OnEventWithArgs;
                break;
        }
    }
}