using DSharpPlus.Entities;

namespace Premium.Utilities.ExtensionMethods;

public static class ColorMethods
{
    public static DiscordColor? ParseColorOrNull(string hex)
    {
        try
        {
            var dcolor = new DiscordColor(hex);
            return dcolor;
        }
        catch (Exception)
        {
            // ignored
        }

        return null;
    }
}