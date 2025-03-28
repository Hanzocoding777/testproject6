using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;

namespace Premium.Logic;

public class ErrorListener
{
    [AsyncListener(EventTypes.SlashCommandErrored)]
    public static async Task ScommandErrorHandler(SlashCommandsExtension slashCommands, SlashCommandErrorEventArgs args)
    {
        switch (args.Exception)
        {
            // PreExecutionCheckFailed - какая либо проверка не прошла (требование админа, команду можно применять только на ботов
            case SlashExecutionChecksFailedException /*when args.Exception is ChecksFailedException*/:
            {
                var builder = new DiscordInteractionResponseBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Произошла ошибка при попытке выполнить команду")
                        .WithDescription($"⚠ Вы исчерпали дневной лимит использования этой команды, вам необходимо подождать перед повторным использованием.")
                        .WithColor(DiscordColor.Yellow)
                    );

                await args.Context.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, builder.AsEphemeral());
                break;
            }

            // Один из необходимых аргументов отсутствует
            case ArgumentNullException:
            {
                var builder = new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Произошла ошибка при попытке выполнить команду")
                        .WithDescription($"⚠ Произошла неизвестная ошибка, обратитесь к разработчику для решения этой проблемы")
                        .WithColor(DiscordColor.Red)
                    );

                await args.Context.EditResponseAsync(builder);
                break;
            }
            
            // В случае неправильно заданного аргумента
            case ArgumentException:
            {
                var builder = new DiscordWebhookBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Произошла ошибка при попытке выполнить команду")
                        .WithDescription($"❌ Не удалось выполнить команду. Проверьте правильность введенных параметров.")
                        .WithColor(DiscordColor.Red)
                    );

                await args.Context.EditResponseAsync(builder);
                break;
            }
        }
    }
}