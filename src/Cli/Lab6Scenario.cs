using Cli.Notifications;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Events;
using Core.Import;
using Core.Notifications;
using Core.Services;
using Core.Storage;

namespace Cli;

internal static class Lab6Scenario
{
    public static int Run(string[] args)
    {
        bool useFile = args.Contains("--file");
        bool useFactory = args.Contains("--factory");
        bool useCache = args.Contains("--cache");
        string dataPath = DataPaths.OrdersFile();

        IOrderStore store = useFactory
            ? StoreFactory.Create(args)
            : useFile
                ? new FileOrderStore(dataPath)
                : new InMemoryOrderStore(SampleData.Orders());

        if (useCache)
            store = new CachingOrderStore(store);

        var service = new OrderService(store);
        string logPath = DataPaths.LogFile();
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        File.WriteAllText(logPath, string.Empty);

        Console.WriteLine("=== Лабораторна 6: доменні події, Action/Func, лямбди ===");
        Console.WriteLine($"Сховище: {store.GetType().Name}");
        Console.WriteLine($"Журнал подій: {logPath}");
        Console.WriteLine();

        void OnOrderChanged(object? sender, OrderChangedEventArgs e)
        {
            string line = OrderChangeText.Format(e);
            Console.WriteLine($"[event] {line}");

            try
            {
                File.AppendAllText(logPath, line + Environment.NewLine);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Console.WriteLine($"  [log] не вдалося записати: {ex.Message}");
            }
        }

        service.Changed += OnOrderChanged;

        int changes = 0;
        var kinds = new List<OrderChangeKind>();
        service.Changed += (_, e) =>
        {
            changes++;
            kinds.Add(e.Kind);
        };

        string runTag = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

        Console.WriteLine("--- 1. Підписка: локальний метод + лямбда-лічильник ---");
        Order created = service.CreateOrder($"CUST-L6-{runTag}");
        Console.WriteLine($"Створено: {created}");
        service.AddLine(created.Id, "PRD-01", "Цемент М400", 150.0m, 2);
        service.AddLine(created.Id, "PRD-02", "Пісок будівельний", 50.0m, 5);
        service.Confirm(created.Id);
        Console.WriteLine($"Після змін: {service.Find(created.Id)}");
        Console.WriteLine($"Подій отримано лямбдою: {changes}");
        Console.WriteLine($"Послідовність: {string.Join(" -> ", kinds)}");
        Console.WriteLine($"Рядків у app.log: {File.ReadAllLines(logPath).Length}");
        Console.WriteLine();

        Console.WriteLine("--- 2. Відписка локального методу через -= ---");
        service.Changed -= OnOrderChanged;
        int changesBeforeCancel = changes;
        int linesBeforeCancel = File.ReadAllLines(logPath).Length;

        service.Cancel(created.Id);

        Console.WriteLine($"Скасовано: {service.Find(created.Id)}");
        Console.WriteLine($"Лічильник лямбди: {changesBeforeCancel} -> {changes} (+1, лямбда ще підписана)");
        Console.WriteLine($"app.log: {linesBeforeCancel} -> {File.ReadAllLines(logPath).Length} рядків (без нових)");
        Console.WriteLine();

        Console.WriteLine("--- 3. Сценарій відмови: виняток є, події немає ---");
        Order empty = service.CreateOrder($"CUST-EMPTY-{runTag}");
        int changesBeforeErrors = changes;
        int linesBeforeErrors = File.ReadAllLines(logPath).Length;

        TryDo("підтвердити порожнє замовлення", () => service.Confirm(empty.Id));
        TryDo("додати рядок з кількістю 0", () => service.AddLine(empty.Id, "PRD-X", "Невірна кількість", 10.0m, 0));
        TryDo("звернутися до неіснуючого замовлення", () => service.Confirm("NOPE-999"));

        Console.WriteLine($"Лічильник: {changesBeforeErrors} -> {changes} (без змін)");
        Console.WriteLine($"app.log: {linesBeforeErrors} -> {File.ReadAllLines(logPath).Length} рядків (без нових)");
        Console.WriteLine();

        Console.WriteLine("--- 4. Func<Order, bool>: пошук за предикатом ---");
        IReadOnlyList<Order> expensive = service.FindAll(o => o.Total > 1000.0m);
        Console.WriteLine($"Замовлень із сумою понад 1000 грн: {expensive.Count}");
        foreach (Order order in expensive.Take(5))
            Console.WriteLine($"  {order}");
        Console.WriteLine($"Підтверджених: {service.FindAll(o => o.Status == OrderStatus.Confirmed).Count}");
        Console.WriteLine();

        Console.WriteLine("--- 5. Стан сховища ---");
        Console.WriteLine($"Усього замовлень: {service.All().Count}");
        foreach (Order order in service.All().Take(5))
            Console.WriteLine($"  {order}");
        Console.WriteLine();

        Console.WriteLine("=== Додаткове 1: INotificationSink + ConsoleSink/FileSink (ручний DI) ===");
        string sinkLogPath = DataPaths.SinkLogFile();
        Directory.CreateDirectory(Path.GetDirectoryName(sinkLogPath)!);
        File.WriteAllText(sinkLogPath, string.Empty);

        INotificationSink sink = new CompositeSink(
            new ConsoleSink(),
            new FileSink(sinkLogPath, reset: false));

        var sinkService = new OrderService(new InMemoryOrderStore(), sink);
        Order sinkOrder = sinkService.CreateOrder($"CUST-SINK-{runTag}");
        sinkService.AddLine(sinkOrder.Id, "PRD-10", "Профіль металевий", 80.0m, 10);
        sinkService.Confirm(sinkOrder.Id);

        Console.WriteLine($"Синхронний журнал сінків: {sinkLogPath} ({File.ReadAllLines(sinkLogPath).Length} рядків)");
        Console.WriteLine("Подій у сервісі з сінком: підписників немає, сповіщення йдуть через конструктор.");
        Console.WriteLine();

        Console.WriteLine("=== Додаткове 2: Action<string> у методі імпорту ===");
        string csvPath = DataPaths.DataFile("sample.csv");
        var skipped = new List<string>();

        ImportResult<ProductDto> imported = ProductCsvImporter.Load(csvPath, message =>
        {
            skipped.Add(message);
            Console.WriteLine($"  пропущено: {message}");
        });

        Console.WriteLine($"Імпортовано товарів: {imported.Items.Count}, пропущено рядків: {skipped.Count}");
        Console.WriteLine($"Помилки також збережено в ImportResult.Errors: {imported.Errors.Count}");
        Console.WriteLine();

        return 0;
    }

    private static void TryDo(string title, Action action)
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
}
