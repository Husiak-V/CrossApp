using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    // DoD 1, 3, 5; Захист 3 (запуск report-top двічі), питання 8 (ThenBy):
    // топ-N сортується за сумою, а ThenBy(Id) робить порядок однаковим для однакових сум.
    public static IReadOnlyList<OrderReportLine> TopOrders(IEnumerable<Order> source, int n)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (n <= 0)
            return [];

        return source
            .OrderByDescending(o => o.Total)
            .ThenBy(o => o.Id, StringComparer.Ordinal)
            .Take(n)
            .Select(o => new OrderReportLine(o.Id, o.CustomerId, o.Status.ToString(), o.Total))
            .ToList();
    }

    // DoD 1, 2, 4: Where + OrderBy + проєкція в DTO; порожнє джерело дає порожній список.
    public static IReadOnlyList<OrderReportLine> AboveTotal(IEnumerable<Order> source, decimal threshold)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .Where(o => o.Total > threshold)
            .OrderBy(o => o.Total)
            .ThenBy(o => o.Id, StringComparer.Ordinal)
            .Select(o => new OrderReportLine(o.Id, o.CustomerId, o.Status.ToString(), o.Total))
            .ToList();
    }

    // DoD 1-2, 4; Захист 1 (GroupBy): g.Key — статус групи, g.Count() — кількість
    // замовлень, g.Sum(...) — сума замовлень цього статусу.
    public static IReadOnlyList<GroupSummary> ByStatus(IEnumerable<Order> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GroupBy(o => o.Status)
            .Select(g => new GroupSummary(g.Key.ToString(), g.Count(), g.Sum(o => o.Total)))
            .OrderByDescending(s => s.Total)
            .ThenBy(s => s.Key, StringComparer.Ordinal)
            .ToList();
    }

    // DoD 1-3: GroupBy за клієнтом, топ-N за сумою і другий ключ для стабільності.
    public static IReadOnlyList<GroupSummary> TopCustomers(IEnumerable<Order> source, int n)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (n <= 0)
            return [];

        return source
            .GroupBy(o => o.CustomerId, StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(o => o.Total)))
            .OrderByDescending(s => s.Total)
            .ThenBy(s => s.Key, StringComparer.Ordinal)
            .Take(n)
            .ToList();
    }

    // DoD 1, 4; питання 7 (Average на порожній послідовності): Average/Max не
    // викликаються для порожнього списку — повертаються нулі без InvalidOperationException.
    public static (int Orders, decimal Total, decimal Average, decimal Max) Totals(IEnumerable<Order> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        List<Order> list = source.ToList();
        return list.Count == 0
            ? (0, 0m, 0m, 0m)
            : (list.Count, list.Sum(o => o.Total), list.Average(o => o.Total), list.Max(o => o.Total));
    }
}
