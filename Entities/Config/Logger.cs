using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;
using Premium.Utilities;

namespace Premium.Entities.Config;

public struct Logger
{
    [JsonProperty("bot"), System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public LogEventLevel Bot { get; private set; }

    [JsonProperty("discord"), System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public LogEventLevel Discord { get; private set; }

    [JsonProperty("database"), System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public LogEventLevel Database { get; private set; }
    public void Load(ServiceCollection services)
    {
        string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

        LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(Bot)
            // Per library settings.
            .MinimumLevel.Override("DSharpPlus", Discord)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Database)
            // Use custom theme because the default one stinks
            .WriteTo.Console(theme: LoggerTheme.Lunar, outputTemplate: outputTemplate);

        Log.Logger = loggerConfiguration.CreateLogger();
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger, true));
        Log.ForContext<Logger>().Information("Logger up!");
    }
}