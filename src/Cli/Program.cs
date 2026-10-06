using Core.Domain;

Console.WriteLine("=== Сценарій 1: успішне створення замовлення та додавання позицій ===");
Order order = Order.Create("ORD-001", "CUST-001");
Console.WriteLine(order);

order.AddLine("PRD-01", "Цемент М400", 150.0m, 2);
order.AddLine("PRD-02", "Пісок будівельний", 50.0m, 5);
Console.WriteLine(order);
foreach (var line in order.Lines)
{
    Console.WriteLine($"  - {line}");
}

order.Confirm();
Console.WriteLine(order);
Console.WriteLine();

Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");
TryDo("додавання рядка до підтвердженого замовлення", () => order.AddLine("PRD-03", "Цегла", 10.0m, 10));

Order emptyOrder = Order.Create("ORD-002", "CUST-002");
TryDo("підтвердження порожнього замовлення", () => emptyOrder.Confirm());

TryDo("некоректна кількість у рядку", () => order.AddLine("PRD-04", "Цвяхи", 5.0m, 0));
TryDo("порожній ID замовлення", () => Order.Create(" ", "CUST-003"));

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}

return 0;
