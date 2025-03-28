using Newtonsoft.Json;
namespace Premium.Entities.Config;

public class Config
{
    [JsonProperty("discord_api_token")]
    internal string DiscordApiToken { get; private set; }
    
    [JsonProperty("crystal_pay_auth_login")]
    internal string CrystalPayAuthLogin { get; private set; }
    
    [JsonProperty("crystal_pay_auth_secret")]
    internal string CrystalPayAuthSecret { get; private set; }

    [JsonProperty("guild_id")]
    internal ulong GuildId { get; private set; }

    [JsonProperty("premium_prices")]
    internal PremiumPrices PremiumPrices { get; private set; }
    
    [JsonProperty("database")]
    internal Database Database { get; private set; }

    [JsonProperty("logger")]
    internal Logger Logger { get; private set; }

    [JsonProperty("channels")]
    internal Channels Channels { get; private set; }

    [JsonProperty("roles")]
    internal Roles Roles { get; private set; }
}