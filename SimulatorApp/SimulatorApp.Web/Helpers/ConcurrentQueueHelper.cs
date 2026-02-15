using System.Collections.Concurrent;

namespace SimulatorApp.Web.Helpers;

public static class ConcurrentQueueHelper
{
    public static List<T> DrainQueue<T>(ConcurrentQueue<T> queue)
    {
        var items = new List<T>();
        while (queue.TryDequeue(out var item))
        {
            items.Add(item);
        }
        return items;
    }
}
