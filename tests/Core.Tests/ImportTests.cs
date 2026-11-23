using Core;
using Core.Domain;
using Core.Import;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public sealed class ImportTests
{
    // DoD 3, 6: заздалегідь скасований токен дає OperationCanceledException, а не DomainException.
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_ThrowsOperationCanceled()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var service = new OrderService(new InMemoryOrderStore());
        Order order = service.CreateOrder("CUST-1", "ORD-I1");

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ImportAsync(new JsonOrderImporter(), order.Id, "any.json", cts.Token));
    }

    // Варіант «Замовлення»: сума перераховується після додавання рядків,
    // подія LineAdded піднімається на кожен доданий рядок.
    [Fact]
    public async Task ImportAsync_AddsLines_AndRecalculatesTotal()
    {
        var service = new OrderService(new InMemoryOrderStore());
        Order order = service.CreateOrder("CUST-1", "ORD-I2");
        var importer = new StubOrderImporter(
        [
            OrderLine.Create("P-1", "Кабель", 10m, 2),
            OrderLine.Create("P-2", "Гвинт", 5m, 4)
        ]);

        int events = 0;
        service.Changed += (_, e) =>
        {
            if (e.Kind == Core.Events.OrderChangeKind.LineAdded)
                events++;
        };

        int added = await service.ImportAsync(importer, order.Id, "unused.json");

        Assert.Equal(2, added);
        Assert.Equal(40m, order.Total);
        Assert.Equal(2, events);
    }

    // DoD 5; захист 4: поганий запис -> DomainException із номером запису.
    [Fact]
    public async Task ImportAsync_WithBrokenRecord_ThrowsDomainException()
    {
        string path = Path.Combine(Path.GetTempPath(), $"crossapp-{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, """
        [
          { "productId": "P-1", "name": "Коректний", "price": 10, "quantity": 1 },
          { "productId": "P-2", "name": "Некоректний", "price": 10, "quantity": 0 }
        ]
        """);

        try
        {
            var service = new OrderService(new InMemoryOrderStore());
            Order order = service.CreateOrder("CUST-1", "ORD-I3");

            DomainException ex = await Assert.ThrowsAsync<DomainException>(() =>
                service.ImportAsync(new JsonOrderImporter(), order.Id, path));

            Assert.Contains("Запис 2", ex.Message);
            Assert.Empty(order.Lines);
        }
        finally
        {
            File.Delete(path);
        }
    }

    // Варіант: імпорт у підтверджене замовлення заборонений бізнес-правилом.
    [Fact]
    public async Task ImportAsync_WhenOrderConfirmed_ThrowsDomainException()
    {
        var service = new OrderService(new InMemoryOrderStore());
        Order order = service.CreateOrder("CUST-1", "ORD-I4");
        service.AddLine(order.Id, "P-1", "Кабель", 10m, 1);
        service.Confirm(order.Id);

        DomainException ex = await Assert.ThrowsAsync<DomainException>(() =>
            service.ImportAsync(new StubOrderImporter([]), order.Id, "unused.json"));

        Assert.Contains("уже підтверджено", ex.Message);
    }

    // Додаткове завдання 1: потоковий імпорт через IAsyncEnumerable + await foreach.
    [Fact]
    public async Task ImportStreamAsync_AddsLinesFromStream()
    {
        var service = new OrderService(new InMemoryOrderStore());
        Order order = service.CreateOrder("CUST-1", "ORD-I5");
        var importer = new StubOrderImporter(
        [
            OrderLine.Create("P-1", "Кабель", 10m, 2),
            OrderLine.Create("P-2", "Гвинт", 5m, 4)
        ]);

        int added = await service.ImportStreamAsync(importer, order.Id, "unused.json");

        Assert.Equal(2, added);
        Assert.Equal(40m, order.Total);
    }

    // Додаткове завдання 1: реальний JsonOrderImporter.LoadStreamAsync
    // і токен, переданий через WithCancellation ([EnumeratorCancellation]).
    [Fact]
    public async Task LoadStreamAsync_ReadsJsonIncrementally()
    {
        string path = Path.Combine(Path.GetTempPath(), $"crossapp-{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, """
        [
          { "productId": "P-1", "name": "Перший", "price": 10, "quantity": 1 },
          { "productId": "P-2", "name": "Другий", "price": 20, "quantity": 2 },
          { "productId": "P-3", "name": "Третій", "price": 30, "quantity": 3 }
        ]
        """);

        try
        {
            using var cts = new CancellationTokenSource();
            var importer = new JsonOrderImporter();
            var ids = new List<string>();

            await foreach (OrderLine line in
                importer.LoadStreamAsync(path).WithCancellation(cts.Token))
            {
                ids.Add(line.ProductId);
            }

            Assert.Equal(new[] { "P-1", "P-2", "P-3" }, ids);
        }
        finally
        {
            File.Delete(path);
        }
    }

    // Додаткове завдання 1: скасування посеред потоку зберігає вже додані рядки.
    [Fact]
    public async Task ImportStreamAsync_WhenCancelledMidStream_StopsAfterDeliveredLine()
    {
        using var cts = new CancellationTokenSource();
        var service = new OrderService(new InMemoryOrderStore());
        Order order = service.CreateOrder("CUST-1", "ORD-I6");

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ImportStreamAsync(
                new CancellingOrderImporter(cts), order.Id, "unused.json", cts.Token));

        Assert.Single(order.Lines);
        Assert.True(cts.IsCancellationRequested);
    }

    // Додаткове завдання 2: доводить, що скасував саме тайм-аут,
    // а не токен користувача (він лишається нескасованим).
    [Fact]
    public async Task ImportAsync_WhenTimeoutElapses_ThrowsImportTimeout_NotUserCancellation()
    {
        var service = new OrderService(new InMemoryOrderStore());
        Order order = service.CreateOrder("CUST-1", "ORD-I7");
        var importer = new BlockingOrderImporter();
        using var userCts = new CancellationTokenSource();

        ImportTimeoutException ex = await Assert.ThrowsAsync<ImportTimeoutException>(() =>
            service.ImportAsync(
                importer, order.Id, "unused.json",
                TimeSpan.FromMilliseconds(100), userCts.Token));

        Assert.False(userCts.IsCancellationRequested);
        Assert.Equal(TimeSpan.FromMilliseconds(100), ex.Timeout);
        Assert.Empty(order.Lines);
    }

    // Додаткове завдання 2: скасування користувачем не маскується під тайм-аут.
    [Fact]
    public async Task ImportAsync_WhenUserCancels_ThrowsOperationCanceled_NotTimeout()
    {
        var service = new OrderService(new InMemoryOrderStore());
        Order order = service.CreateOrder("CUST-1", "ORD-I8");
        var importer = new BlockingOrderImporter();
        using var userCts = new CancellationTokenSource();

        Task<int> import = service.ImportAsync(
            importer, order.Id, "unused.json",
            TimeSpan.FromSeconds(30), userCts.Token);

        await importer.Started;
        await userCts.CancelAsync();

        Exception ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => import);

        Assert.IsNotType<ImportTimeoutException>(ex);
        Assert.True(userCts.IsCancellationRequested);
    }
}
