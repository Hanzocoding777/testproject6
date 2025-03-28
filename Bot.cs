using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System.Reflection;
using System.Text;
using Database.Services;
using Premium.Api;
using Premium.Entities.Config;
using Premium.Listeners;
using Premium.Logic;

namespace Premium;

public class Bot
{
#nullable disable
    public static Config Config { get; private set; }
    public static DiscordClient Client { get; private set; }
    public static DiscordGuild Guild { get; set; }
    public static ServiceProvider ServiceProvider { get; private set; }
    public static SlashCommandsExtension SlashCommands { get; private set; }
    public static CrystalPayApi CrystalPay { get; private set; }
#nullable enable

    private static void LoadSettings()
    {
        if (!File.Exists("config.json"))
        {
            var json = JsonConvert.SerializeObject(new Config(), Formatting.Indented);
            File.WriteAllText("config.json", json, new UTF8Encoding(false));
            Console.WriteLine("Config file was not found, a new one was generated. Fill it with proper values and rerun this program");
            Console.ReadKey();
            return;
        }

        var input = File.ReadAllText("config.json", new UTF8Encoding(false));
        Config = JsonConvert.DeserializeObject<Config>(input)!;

        // Saving config with same values but updated fields
        var newjson = JsonConvert.SerializeObject(Config, Formatting.Indented);
        File.WriteAllText("config.json", newjson, new UTF8Encoding(false));
    }

    private static void Main()
    {
        LoadSettings();
        var bot = new Bot();
        bot.MainAsync().GetAwaiter().GetResult();
    }

    private async Task MainAsync()
    {
        ServiceCollection services = new();
        Config.Logger.Load(services);

        ServiceProvider = services.BuildServiceProvider();
        var logger = Log.Logger.ForContext<Bot>();

        var config = new DiscordConfiguration()
        {
            Token = Config.DiscordApiToken,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            LoggerFactory = ServiceProvider.GetService<ILoggerFactory>(),
            LogUnknownEvents = false //  ignore unknow event GUILD_AUDIT_LOG_ENTRY_CREATE in D# 4.3.0
        };

        var discordClient = new DiscordClient(config);
        Client = discordClient;

        Guild = await discordClient.GetGuildAsync(Bot.Config.GuildId);

        var slashCommands = discordClient.UseSlashCommands(new SlashCommandsConfiguration());
        slashCommands.RegisterCommands(Assembly.GetExecutingAssembly());
        SlashCommands = slashCommands;
        logger.Information("Commands registered");

        discordClient.Zombied += (client, _) => client.ReconnectAsync();

        slashCommands.SlashCommandErrored += (sender, args) =>
        {
            Console.WriteLine(args.Exception);
            return Task.CompletedTask;
        };
        
        AsyncListenerHandler.InstallListeners(Client, this);

        logger.Information("Initializing database...");
        ServiceProvider = ConfigureServices();
        await ServiceProvider.GetRequiredService<MongoManager>().InitializeAsync(Bot.Config.Database.ConnectionString, Bot.Config.Database.DatabaseName);
        logger.Information("Database initialized");

        logger.Information("Initializing CrystalPay...");
        CrystalPay = new(Bot.Config.CrystalPayAuthLogin, Bot.Config.CrystalPayAuthSecret);
        logger.Information("CrystalPay initialized");
        
        await Timers.RegisterTimers();
        
        await discordClient.ConnectAsync();
        logger.Information("Logged in {CurrentUserUsername}", discordClient.CurrentUser.Username);
        await Task.Delay(-1);
    }
    
    private ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton(Client)
            .AddSingleton<MongoManager>()
            .BuildServiceProvider();
    }
}