using Newtonsoft.Json;

namespace Premium.Entities.Config;

public struct Database
{
    [JsonProperty("connection_string")] public string ConnectionString { get; private set; }

    [JsonProperty("database_name")] public string DatabaseName { get; private set; }
}