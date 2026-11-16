using Core.Domain;

namespace Cli;

internal static class DeferredExecutionDemo
{
    // DoD 6; питання 2, 4, 5: Where лише описує запит, Count() запускає перебирання,
    // ToList() фіксує результат. Для відтворення беремо локальний список, бо
    // OrderService.All() кожного разу повертає новий знімок.
    public static void Run(IReadOnlyList<Order> sourceSnapshot, decimal minTotal)
    {
        List<Order> source = sourceSnapshot.ToList();
        IEnumerable<Order> query = source.Where(o => o.Total > minTotal);

        int before = query.Count();

        source.Add(NewDemoOrder());
        int after = query.Count();

        List<Order> snapshot = query.ToList();
        source.Clear();

        Console.WriteLine($"{"COUNT(before)",-15} {before,4}");
        Console.WriteLine($"{"COUNT(after)",-15} {after,4}");
        Console.WriteLine($"{"SNAPSHOT",-15} {snapshot.Count,4}");
        Console.WriteLine($"{"SOURCE(cleared)",-15} {source.Count,4}");
    }

    private static Order NewDemoOrder()
    {
        Order order = Order.Create("ORD-DEMO-01", "CUST-DEMO");
        order.AddLine("PRD-DEMO", "Демо-позиція", 5000m, 3);
        return order;
    }
}
