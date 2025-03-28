using Newtonsoft.Json;

namespace Premium.Entities.Config;

public struct Channels
{
    [JsonProperty("error_channel_id")]
    public ulong ErrorChannelId { get; private set; }
    
    [JsonProperty("premium_log_channel_id")]
    public ulong PremiumLogChannelId { get; private set; }
    
    [JsonProperty("create_premium_channel_id")]
    public ulong CreatePremiumChannelId { get; private set; }
    
    [JsonProperty("premium_parent_id")]
    public ulong PremiumParentId { get; private set; }
    
    [JsonProperty("main_chat_channel_id")]
    public ulong MainChatChannelId { get; private set; }
    
    [JsonProperty("trash_channel_id")]
    public ulong TrashChannelId { get; private set; }
    
    [JsonProperty("premium_panel_channel_id")]
    public ulong PremiumPanelChannelId { get; private set; }
}