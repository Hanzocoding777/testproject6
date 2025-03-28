using DSharpPlus.Entities;
using Serilog;

namespace Premium.Entities;

public class ErrorMessageSender
{
    private static readonly ILogger Logger = Log.ForContext<ErrorMessageSender>();

    public static async Task SendError(string message, Exception exception)
    {
        var guild = await Bot.Client.GetGuildAsync(Bot.Config.GuildId);

        var errorChannel = guild.GetChannel(Bot.Config.Channels.ErrorChannelId);

        string ex = $"**{message}** \n" +
                    $"**Exception:** {exception.GetType()}: {exception.Message} \n" +
                    $"**StackTrace:** ```{exception.StackTrace}```";

        if (ex.Length < 2000)
        {
            var builder = new DiscordMessageBuilder()
                .WithContent(ex);

            await builder.SendAsync(errorChannel);
        }
        else
        {
            using var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);

            try
            {
                await streamWriter.WriteAsync(exception.StackTrace);
                await streamWriter.FlushAsync();
                memoryStream.Seek(0, SeekOrigin.Begin);

                var errorChannelBuilder = new DiscordMessageBuilder()
                    .WithContent($"**{message}** \n" +
                                 $"**Exception:** {exception.GetType()}: {exception.Message} \n")
                    .AddFile($"exception.txt", memoryStream);

                await errorChannelBuilder.SendAsync(errorChannel);
            }
            catch (Exception sendException)
            {
                Console.WriteLine($"{exception} \n \n {sendException}");
                await File.WriteAllTextAsync($@"exceptions\exception-{DateTime.Now:d.M-m-H}.txt", $"{exception} \n \n {sendException}");
            }
        }
    }

    public static async Task SendError(Exception exception)
    {
        var guild = await Bot.Client.GetGuildAsync(Bot.Config.GuildId);

        var errorChannel = guild.GetChannel(Bot.Config.Channels.ErrorChannelId);

        string ex = $"**Exception:** {exception.GetType()}: {exception.Message} \n" +
                    $"**StackTrace:** ```{exception.StackTrace}```";

        if (ex.Length < 2000)
        {
            var builder = new DiscordMessageBuilder()
                .WithContent(ex);

            await builder.SendAsync(errorChannel);
        }
        else
        {
            using var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);

            try
            {
                await streamWriter.WriteAsync(exception.StackTrace);
                await streamWriter.FlushAsync();
                memoryStream.Seek(0, SeekOrigin.Begin);

                var errorChannelBuilder = new DiscordMessageBuilder()
                    .WithContent($"**Exception:** {exception.GetType()}: {exception.Message} \n")
                    .AddFile($"exception.txt", memoryStream);

                await errorChannelBuilder.SendAsync(errorChannel);
            }
            catch (Exception sendException)
            {
                Logger.Error("{exception}", $"{exception} \n \n {sendException}");
                await File.WriteAllTextAsync($@"exceptions\exception-{DateTime.Now:d.M-m-H}.txt", $"{exception} \n \n {sendException}");
            }
        }
    }
}