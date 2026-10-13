using Core.Domain;
using Core.Dto;

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
Console.WriteLine($"Стан після всіх відмов лишається: {order}");
Console.WriteLine();

OrderDto dto = order.ToDto();
Order restored = Order.FromDto(dto);
Console.WriteLine($"ToDto/FromDto: {restored}");
Console.WriteLine();

Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");
TryDo("додавання рядка до підтвердженого замовлення", () => order.AddLine("PRD-03", "Цегла", 10.0m, 10));

Order emptyOrder = Order.Create("ORD-002", "CUST-002");
TryDo("підтвердження порожнього замовлення", () => emptyOrder.Confirm());

Order draft = Order.Create("ORD-003", "CUST-003");
TryDo("некоректна кількість у рядку", () => draft.AddLine("PRD-04", "Цвяхи", 5.0m, 0));
TryDo("від'ємна ціна у рядку", () => draft.AddLine("PRD-04", "Цвяхи", -5.0m, 1));
TryDo("порожній ID замовлення", () => Order.Create(" ", "CUST-003"));
TryDo("скасування вже скасованого", () =>
{
    draft.Cancel();
    draft.Cancel();
});
Console.WriteLine();

Console.WriteLine("=== Додаткове 1: ImportResult -> сутності + помилки інваріантів ===");
var import = new ImportResult<OrderLineDto>(
    [
        new OrderLineDto("PRD-10", "Цемент", 100.0m, 2),
        new OrderLineDto("PRD-11", "Пісок", 20.0m, 0), 
        new OrderLineDto(" ", "Без ID", 10.0m, 1),     
    ],
    ["рядок 7: ціна 'abc' не є коректним числом"]);
ImportResult<OrderLine> converted = OrderImport.ToEntities(import);
Console.WriteLine($"Валідних сутностей: {converted.Items.Count}, помилок: {converted.Errors.Count}");
foreach (string err in converted.Errors)
    Console.WriteLine($"  - {err}");
Console.WriteLine();

Console.WriteLine("=== Додаткове 2: правило двох сутностей (ліміт 5 відкритих замовлень) ===");
var existing = new List<Order>();
for (int i = 1; i <= 5; i++)
{
    Order o = Order.Create($"ORD-10{i}", "CUST-007");
    o.AddLine("PRD-01", "Цемент", 10.0m, 1);
    o.Confirm();
    existing.Add(o);
}
TryDo("6-те відкрите замовлення клієнта", () => OrderPolicy.EnsureCanCreateOrder(existing, "CUST-007"));
Console.WriteLine("Пояснення: правило потребує доступу до сховища всіх замовлень, тому живе в сервісі (тиждень 5), а не в сутності.");
Console.WriteLine();

Console.WriteLine("=== Додаткове 3: enum OrderStatus + switch-переходи ===");
Order cancellable = Order.Create("ORD-020", "CUST-020");
cancellable.AddLine("PRD-01", "Цемент", 10.0m, 1);
cancellable.Confirm();
cancellable.Cancel();
Console.WriteLine($"Confirm -> Cancel: {cancellable.Status}");
TryDo("перехід Cancelled -> Confirmed", () => cancellable.Confirm());

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
