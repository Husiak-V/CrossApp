using Core.Domain;

namespace Core.Tests;

// Фіксований набір із проєкту тестів, а не SampleData (DoD 6).
internal static class TestData
{
    public static IReadOnlyList<Order> Orders() =>
    [
        Draft("ORD-A", "CUST-1", ("P-1", "Товар A", 100m, 2)),   // 200
        Confirmed("ORD-B", "CUST-2", ("P-2", "Товар B", 50m, 1)), // 50
        Confirmed("ORD-C", "CUST-1", ("P-3", "Товар C", 25m, 4)), // 100
        Draft("ORD-D", "CUST-3", ("P-4", "Товар D", 10m, 5))      // 50
    ];

    private static Order Draft(
        string id, string customerId,
        params (string ProductId, string Name, decimal Price, int Quantity)[] lines)
    {
        Order order = Order.Create(id, customerId);
        foreach ((string productId, string name, decimal price, int quantity) in lines)
            order.AddLine(productId, name, price, quantity);

        return order;
    }

    private static Order Confirmed(
        string id, string customerId,
        params (string ProductId, string Name, decimal Price, int Quantity)[] lines)
    {
        Order order = Draft(id, customerId, lines);
        order.Confirm();
        return order;
    }
}
