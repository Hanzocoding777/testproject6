using Newtonsoft.Json;

namespace Premium.Entities.Config;

public struct PremiumPrices
{
    [JsonProperty("premium1_price")]
    public int Premium1Price { get; private set; }
    
    [JsonProperty("premium2_price")]
    public int Premium2Price { get; private set; }
    
    [JsonProperty("premium3_price")]
    public int Premium3Price { get; private set; }
}