using Newtonsoft.Json;

namespace Premium.Entities.Config;

public struct Roles
{
    [JsonProperty("highest_role_id")]
    public ulong HighestRoleId { get; private set; }
    
    [JsonProperty("premium1_role_id")]
    public ulong Premium1RoleId { get; private set; }
    
    [JsonProperty("premium2_role_id")]
    public ulong Premium2RoleId { get; private set; }
    
    [JsonProperty("premium3_role_id")]
    public ulong Premium3RoleId { get; private set; }
    
    [JsonProperty("live_stream_role_id")]
    public ulong LiveStreamRoleId { get; private set; }
}