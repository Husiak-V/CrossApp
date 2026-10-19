using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");
bool useFactory = args.Contains("--factory");
bool useCache = args.Contains("--cache");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "orders.json");

IOrderStore store = useFactory
    ? StoreFactory.Create(args)
    : useFile
        ? new FileOrderStore(dataPath)
        : new InMemoryOrderStore(SampleData.Orders());

if (useCache)
    store = new CachingOrderStore(store);

var service = new OrderService(store);
Console.WriteLine($"Сховище: {store.GetType().Name}");
Console.WriteLine();

Console.WriteLine("=== Сценарій: створення, зміна, список, пошук ===");
Order created = service.CreateOrder("CUST-100");
Console.WriteLine($"Створено: {created}");

service.AddLine(created.Id, "PRD-01", "Цемент М400", 150.0m, 2);
service.AddLine(created.Id, "PRD-02", "Пісок будівельний", 50.0m, 5);
service.Confirm(created.Id);
Console.WriteLine($"Після змін: {service.Find(created.Id)}");

Order? found = service.Find(created.Id);
Console.WriteLine($"Пошук за id '{created.Id}':");
foreach (OrderLine line in found!.Lines)
    Console.WriteLine($"    {line}");

Console.WriteLine();
Console.WriteLine($"Усього замовлень у сховищі: {service.All().Count}");
foreach (Order order in service.All())
    Console.WriteLine($"  {order}");

Console.WriteLine();
Console.WriteLine("=== Додаткове 2: пошук за предикатом Func<Order, bool> ===");
Console.WriteLine($"Підтверджених: {service.Find(o => o.Status == OrderStatus.Confirmed).Count}");
Console.WriteLine($"Із сумою понад 300 грн: {service.Find(o => o.Total > 300m).Count}");
Console.WriteLine();

service.Cancel(created.Id);
Console.WriteLine($"Демонстраційне замовлення скасовано: {service.Find(created.Id)}");
Console.WriteLine();

Console.WriteLine("=== Сценарій відмови ===");
TryDo("дубль id", () =>
{
    service.CreateOrder("CUST-200", "DUP-001");
    service.CreateOrder("CUST-200", "DUP-001");
});
TryDo("операція з неіснуючим id", () => service.Confirm("NOPE-999"));

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював — перевірки відсутні!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}

return 0;
