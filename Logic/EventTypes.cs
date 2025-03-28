namespace Premium.Logic;

public enum EventTypes
{
    /// <summary>
    /// 
    /// </summary>
    AutocompleteErrored,

    /// <summary>
    /// 
    /// </summary>
    AutocompleteExecuted,

    /// <summary>
    /// Fired when a new channel is created. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    ChannelCreated,

    /// <summary>
    /// Fired when a channel is deleted For this Event you need the Guilds intent specified in Intents
    /// </summary>
    ChannelDeleted,

    /// <summary>
    /// Fired whenever a channel's pinned message list is updated. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    ChannelPinsUpdated,

    /// <summary>
    /// Fired when a channel is updated. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    ChannelUpdated,

    /// <summary>
    /// Fired whenever an error occurs within an event handler.
    /// </summary>
    ClientErrored,

    /// <summary>
    /// Fired when a component is invoked.
    /// </summary>
    ComponentInteractionCreated,

    /// <summary>
    /// Fired when a user uses a context menu.
    /// </summary>
    ContextMenuInteractionCreated,

    /// <summary>
    /// Fire when the execution of a context menu is successful.
    /// </summary>
    ContextMenuExecuted,

    /// <summary>
    /// Fires when the execution of a context menu fails.
    /// </summary>
    ContextMenuErrored,

    /// <summary>
    /// Fired when a context menu has been received and is to be executed
    /// </summary>
    ContextMenuInvoked,

    /// <summary>
    /// Fired when a dm channel is deleted For this Event you need the Direct​Messages intent specified in Intents
    /// </summary>
    DmChannelDeleted,

    /// <summary>
    /// Fired when a guild is becoming available. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    GuildAvailable,

    /// <summary>
    /// Fired when a guild ban gets added For this Event you need the Guild​Bans intent specified in Intents
    /// </summary>
    GuildBanAdded,

    /// <summary>
    /// Fired when a guild ban gets removed For this Event you need the Guild​Bans intent specified in Intents
    /// </summary>
    GuildBanRemoved,

    /// <summary>
    /// Fired when the user joins a new guild. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    GuildCreated,

    /// <summary>
    /// Fired when the user leaves or is removed from a guild. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    GuildDeleted,

    /// <summary>
    /// Fired when all guilds finish streaming from Discord.
    /// </summary>
    GuildDownloadCompleted,

    /// <summary>
    /// Fired when a guilds emojis get updated For this Event you need the Guild​Emojis intent specified in Intents
    /// </summary>
    GuildEmojisUpdated,

    /// <summary>
    /// Fired when a guild integration is updated.
    /// </summary>
    GuildIntegrationsUpdated,

    /// <summary>
    /// Fired when a new user joins a guild. For this Event you need the Guild​Members intent specified in Intents
    /// </summary>
    GuildMemberAdded,

    /// <summary>
    /// Fired when a user is removed from a guild (leave/kick/ban). For this Event you need the Guild​Members intent specified in Intents
    /// </summary>
    GuildMemberRemoved,

    /// <summary>
    /// Fired in response to Gateway Request Guild Members.
    /// </summary>
    GuildMembersChunked,

    /// <summary>
    /// Fired when a guild member is updated. For this Event you need the Guild​Members intent specified in Intents
    /// </summary>
    GuildMemberUpdated,

    /// <summary>
    /// Fired when a guild role is created. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    GuildRoleCreated,

    /// <summary>
    /// Fired when a guild role is updated. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    GuildRoleDeleted,

    /// <summary>
    /// Fired when a guild role is updated. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    GuildRoleUpdated,

    /// <summary>
    /// Fired when a guild stickers is updated. For this Event you need the **** intent specified in Intents
    /// </summary>
    GuildStickersUpdated,

    /// <summary>
    /// Fired when a guild becomes unavailable.
    /// </summary>
    GuildUnavailable,

    /// <summary>
    /// Fired when a guild is updated. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    GuildUpdated,

    /// <summary>
    /// Fired on received heartbeat ACK.
    /// </summary>
    Heartbeated,

    /// <summary>
    /// Fired when an integration is created.
    /// </summary>
    IntegrationCreated,

    /// <summary>
    /// Fired when an integration is deleted.
    /// </summary>
    IntegrationDeleted,

    /// <summary>
    /// Fired when an integration is updated.
    /// </summary>
    IntegrationUpdated,

    /// <summary>
    /// Fired when an interaction is invoked.
    /// </summary>
    InteractionCreated,

    /// <summary>
    /// Fired when an invite is created. For this Event you need the Guild​Invites intent specified in Intents
    /// </summary>
    InviteCreated,

    /// <summary>
    /// Fired when an invite is deleted. For this Event you need the Guild​Invites intent specified in Intents
    /// </summary>
    InviteDeleted,

    /// <summary>
    /// Fired when message is acknowledged by the user. For this Event you need the Guild​Messages intent specified in Intents
    /// </summary>
    MessageAcknowledged,

    /// <summary>
    /// Fired when a message is created. For this Event you need the Guild​Messages intent specified in Intents
    /// </summary>
    MessageCreated,

    /// <summary>
    /// Fired when a message is deleted. For this Event you need the Guild​Messages intent specified in Intents
    /// </summary>
    MessageDeleted,

    /// <summary>
    /// Fired when a reaction gets added to a message. For this Event you need the Guild​Message​Reactions intent specified in Intents
    /// </summary>
    MessageReactionAdded,

    /// <summary>
    /// Fired when a reaction gets removed from a message. For this Event you need the Guild​Message​Reactions intent specified in Intents
    /// </summary>
    MessageReactionRemoved,

    /// <summary>
    /// Fired when all reactions of a specific reaction are removed from a message. For this Event you need the Guild​Message​Reactions intent specified in Intents
    /// </summary>
    MessageReactionRemovedEmoji,

    /// <summary>
    /// Fired when all reactions get removed from a message. For this Event you need the Guild​Message​Reactions intent specified in Intents
    /// </summary>
    MessageReactionsCleared,

    /// <summary>
    /// Fired when multiple messages are deleted at once. For this Event you need the Guild​Messages intent specified in Intents
    /// </summary>
    MessagesBulkDeleted,

    /// <summary>
    /// Fired when a message is updated. For this Event you need the Guild​Messages intent specified in Intents
    /// </summary>
    MessageUpdated,

    /// <summary>
    /// Fired when a modal is submitted. If a modal is closed, this event is not fired.
    /// </summary>
    ModalSubmitted,

    /// <summary>
    /// Fired when a presence has been updated. For this Event you need the Guild​Presences intent specified in Intents
    /// </summary>
    PresenceUpdated,

    /// <summary>
    /// Fired when this client has successfully completed its handshake with the websocket gateway.
    /// </summary>
    Ready,

    /// <summary>
    /// Fired whenever a session is resumed.
    /// </summary>
    Resumed,

    /// <summary>
    /// 
    /// </summary>
    ScheduledGuildEventCompleted,

    /// <summary>
    /// 
    /// </summary>
    ScheduledGuildEventCreated, 

    /// <summary>
    /// 
    /// </summary>
    ScheduledGuildEventDeleted,

    /// <summary>
    /// 
    /// </summary>
    ScheduledGuildEventUpdated,

    /// <summary>
    /// 
    /// </summary>
    ScheduledGuildEventUserAdded,

    /// <summary>
    /// 
    /// </summary>
    ScheduledGuildEventUserRemoved,

    /// <summary>
    /// Fires when the execution of a slash command fails.
    /// </summary>
    SlashCommandErrored,

    /// <summary>
    /// Fires when the execution of a slash command is successful.
    /// </summary>
    SlashCommandExecuted,

    /// <summary>
    /// Fired when a slash command has been received and is to be executed
    /// </summary>
    SlashCommandInvoked,

    /// <summary>
    /// Fired whenever WebSocket connection is terminated.
    /// </summary>
    SocketClosed,

    /// <summary>
    /// Fired whenever a WebSocket error occurs within the client.
    /// </summary>
    SocketErrored,

    /// <summary>
    /// Fired whenever WebSocket connection is established.
    /// </summary>
    SocketOpened,

    /// <summary>
    /// Fired when a stage instance is created.
    /// </summary>
    StageInstanceCreated,

    /// <summary>
    /// Fired when a stage instance is deleted.
    /// </summary>
    StageInstanceDeleted,

    /// <summary>
    /// Fired when a stage instance is updated.
    /// </summary>
    StageInstanceUpdated,

    /// <summary>
    /// Fired when a thread is created. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    ThreadCreated,

    /// <summary>
    /// Fired when a thread is deleted. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    ThreadDeleted,

    /// <summary>
    /// Fired when the current member gains access to a channel(s) that has threads. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    ThreadListSynced,

    /// <summary>
    /// Fired when the thread members are updated. For this Event you need the Guild​Members or Guilds intent specified in Intents
    /// </summary>
    ThreadMembersUpdated,

    /// <summary>
    /// Fired when the thread member for the current user is updated. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    ThreadMemberUpdated,

    /// <summary>
    /// Fired when a thread is updated. For this Event you need the Guilds intent specified in Intents
    /// </summary>
    ThreadUpdated,

    /// <summary>
    /// Fired when a user starts typing in a channel.
    /// </summary>
    TypingStarted,

    /// <summary>
    /// Fired when an unknown event gets received.
    /// </summary>
    UnknownEvent,

    /// <summary>
    /// Fired when the current user updates their settings. For this Event you need the Guild​Presences intent specified in Intents
    /// </summary>
    UserSettingsUpdated,

    /// <summary>
    /// Fired when properties about the current user change.
    /// </summary>
    UserUpdated,

    /// <summary>
    /// Fired when a guild's voice server is updated. For this Event you need the Guild​Voice​States intent specified in Intents
    /// </summary>
    VoiceServerUpdated,

    /// <summary>
    /// Fired when someone joins/leaves/moves voice channels. For this Event you need the Guild​Voice​States intent specified in Intents
    /// </summary>
    VoiceStateUpdated,

    /// <summary>
    /// Fired whenever webhooks update.
    /// </summary>
    WebhooksUpdated,

    /// <summary>
    /// Fired on heartbeat attempt cancellation due to too many failed heartbeats.
    /// </summary>
    Zombied
}