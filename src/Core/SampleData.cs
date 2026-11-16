using Core.Domain;

namespace Core;

public static class SampleData
{
    // DoD 8: 20 замовлень і 3 статуси — достатньо для GroupBy, топ-N і агрегатів.
    public static IReadOnlyList<Order> Orders() =>
    [
        Confirmed("ORD-1001", "CUST-001", ("PRD-01", "Цемент М400", 150.0m, 20), ("PRD-02", "Пісок будівельний", 50.0m, 5)),
        Draft("ORD-1002", "CUST-001", ("PRD-03", "Цегла рядова", 10.0m, 100)),
        Confirmed("ORD-1003", "CUST-002", ("PRD-04", "Кабель UTP cat6", 25.0m, 50)),
        Cancelled("ORD-1004", "CUST-002", ("PRD-05", "Цвяхи будівельні", 5.0m, 10)),
        Draft("ORD-1005", "CUST-003", ("PRD-06", "Цемент М500", 170.0m, 30)),
        Draft("ORD-1006", "CUST-003", ("PRD-07", "Бетон товарний", 900.0m, 3)),
        Confirmed("ORD-1007", "CUST-004", ("PRD-04", "Кабель UTP cat6", 25.0m, 100), ("PRD-08", "Конектор RJ-45", 3.0m, 200)),
        Confirmed("ORD-1008", "CUST-004", ("PRD-09", "Гіпсокартон 12.5 мм", 120.0m, 15)),
        Draft("ORD-1009", "CUST-005", ("PRD-10", "Профіль металевий", 80.0m, 40)),
        Cancelled("ORD-1010", "CUST-005", ("PRD-11", "Фарба фасадна", 200.0m, 4)),
        Confirmed("ORD-1011", "CUST-006", ("PRD-01", "Цемент М400", 150.0m, 50)),
        Draft("ORD-1012", "CUST-006", ("PRD-02", "Пісок будівельний", 50.0m, 20)),
        Confirmed("ORD-1013", "CUST-001", ("PRD-03", "Цегла рядова", 10.0m, 500)),
        Draft("ORD-1014", "CUST-002", ("PRD-12", "Утеплювач мінеральний", 300.0m, 8)),
        Confirmed("ORD-1015", "CUST-003", ("PRD-07", "Бетон товарний", 900.0m, 5)),
        Draft("ORD-1016", "CUST-004", ("PRD-09", "Гіпсокартон 12.5 мм", 120.0m, 20)),
        Confirmed("ORD-1017", "CUST-005", ("PRD-10", "Профіль металевий", 80.0m, 60)),
        Cancelled("ORD-1018", "CUST-006", ("PRD-11", "Фарба фасадна", 200.0m, 10)),
        Confirmed("ORD-1019", "CUST-007", ("PRD-13", "Штукатурка гіпсова", 180.0m, 12)),
        Draft("ORD-1020", "CUST-007", ("PRD-14", "Плитка керамічна", 250.0m, 6))
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

    private static Order Cancelled(
        string id, string customerId,
        params (string ProductId, string Name, decimal Price, int Quantity)[] lines)
    {
        Order order = Draft(id, customerId, lines);
        order.Cancel();
        return order;
    }
}
