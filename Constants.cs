using DSharpPlus.Entities;

namespace Premium;

public static class Constants
{
    public static readonly DiscordEmoji Pencil = DiscordEmoji.FromGuildEmote(Bot.Client, 1345140325345267734);
    public static readonly DiscordEmoji Add = DiscordEmoji.FromGuildEmote(Bot.Client, 1345154006229712927);
    public static readonly DiscordEmoji Lock = DiscordEmoji.FromGuildEmote(Bot.Client, 1345140291564339321);
    public static readonly DiscordEmoji RejectUser = DiscordEmoji.FromGuildEmote(Bot.Client, 1345140335717777511);
    public static readonly DiscordEmoji InviteUser = DiscordEmoji.FromGuildEmote(Bot.Client, 1345140278390030387);
    public static readonly DiscordEmoji Microphone = DiscordEmoji.FromGuildEmote(Bot.Client, 1345140314192482457);
    public static readonly DiscordEmoji HideEye = DiscordEmoji.FromGuildEmote(Bot.Client, 1345141623624826961);
    public static readonly DiscordEmoji Mute = DiscordEmoji.FromGuildEmote(Bot.Client, 1345140301903167488);
    public static readonly DiscordEmoji Paint = DiscordEmoji.FromGuildEmote(Bot.Client, 1345140258097856675);
    
    public static readonly DiscordColor MainColor = new("006090");
    public static readonly DiscordColor InvisibleColor = new DiscordColor("#31333a");

    public const string EmptyString = "\u200B";
    
    public const string EmptyLineImageUrl = "https://i.imgur.com/KobSoam.png";
}