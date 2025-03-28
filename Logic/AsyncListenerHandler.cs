using DSharpPlus;
using System.Reflection;

namespace Premium.Logic;

public static class AsyncListenerHandler
{
    public static IEnumerable<ListenerMethod> ListenerMethods { get; private set; }

    public static void InstallListeners(DiscordClient client, Bot bot)
    {
        // find all methods bot with AsyncListener attr
        ListenerMethods = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes(), (a, t) => new { a, t })
            .SelectMany(t1 => t1.t.GetMethods(), (t1, m) => new { t1, m })
            .Select(t1 => new { t1, attribute = t1.m.GetCustomAttribute(typeof(AsyncListenerAttribute), true) })
            .Where(t1 => t1.attribute != null)
            .Select(t1 =>
                new ListenerMethod { Method = t1.t1.m, Attribute = (t1.attribute as AsyncListenerAttribute)! });

        foreach (var listener in ListenerMethods)
        {
            listener.Attribute.Register(bot, client, listener.Method);
        }
    }
}

public class ListenerMethod
{
    public MethodInfo Method { get; internal set; }
    public AsyncListenerAttribute Attribute { get; internal set; }
}