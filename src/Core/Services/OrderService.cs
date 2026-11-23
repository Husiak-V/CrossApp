using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Import;
using Core.Notifications;

namespace Core.Services;

public sealed class OrderService
{
    private readonly IOrderStore _store;
    private readonly INotificationSink? _notificationSink;

    public OrderService(IOrderStore store, INotificationSink? notificationSink = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _notificationSink = notificationSink;
    }

    public event EventHandler<OrderChangedEventArgs>? Changed;

    public Order CreateOrder(string customerId, string? orderId = null)
    {
        OrderPolicy.EnsureCanCreateOrder(_store.List(), customerId);

        string id = string.IsNullOrWhiteSpace(orderId)
            ? $"ORD-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}"
            : orderId;

        Order order = Order.Create(id, customerId);
        _store.Add(order);
        OnChanged(order, OrderChangeKind.OrderCreated);
        return order;
    }

    public void AddLine(string orderId, string productId, string name, decimal price, int quantity)
    {
        Order order = Require(orderId);
        order.AddLine(productId, name, price, quantity);
        _store.Update(order);
        OnChanged(order, OrderChangeKind.LineAdded);
    }

    public void Confirm(string orderId)
    {
        Order order = Require(orderId);
        order.Confirm();
        _store.Update(order);
        OnChanged(order, OrderChangeKind.OrderConfirmed);
    }

    public void Cancel(string orderId)
    {
        Order order = Require(orderId);
        order.Cancel();
        _store.Update(order);
        OnChanged(order, OrderChangeKind.OrderCancelled);
    }

    public bool Remove(string orderId)
    {
        Order? order = _store.GetById(orderId);
        if (order is null)
            return false;

        bool removed = _store.Remove(orderId);
        if (removed)
            OnChanged(order, OrderChangeKind.OrderRemoved);

        return removed;
    }

    public IReadOnlyList<Order> All() => _store.List();

    public Order? Find(string id) => _store.GetById(id);

    public IReadOnlyList<Order> FindAll(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    public IReadOnlyList<Order> Find(Func<Order, bool> predicate) => FindAll(predicate);

    // DoD 1, 3; захист 2: ct проходить ланцюжком Cli -> ImportAsync -> LoadAsync.
    public async Task<int> ImportAsync(
        IOrderImporter importer, string orderId, string path, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(importer);
        Order order = RequireImportableOrder(orderId);

        IReadOnlyList<OrderLine> lines = await importer.LoadAsync(path, ct);
        return ApplyLines(order, lines, ct);
    }

    // Додаткове завдання 1: потоковий імпорт; рядок додається одразу після yield.
    public async Task<int> ImportStreamAsync(
        IOrderImporter importer, string orderId, string path, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(importer);
        Order order = RequireImportableOrder(orderId);

        int added = 0;
        await foreach (OrderLine line in importer.LoadStreamAsync(path, ct).WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            if (TryAddLine(order, line))
                added++;
        }

        return added;
    }

    // Додаткове завдання 2: зв'язаний токен користувача і тайм-ауту; якщо
    // скасувався саме timeoutCts, назовні йде ImportTimeoutException.
    public Task<int> ImportAsync(
        IOrderImporter importer, string orderId, string path, TimeSpan timeout,
        CancellationToken userToken = default)
    {
        ArgumentNullException.ThrowIfNull(importer);
        return WithTimeoutAsync(
            ct => ImportAsync(importer, orderId, path, ct), timeout, userToken);
    }

    public Task<int> ImportStreamAsync(
        IOrderImporter importer, string orderId, string path, TimeSpan timeout,
        CancellationToken userToken = default)
    {
        ArgumentNullException.ThrowIfNull(importer);
        return WithTimeoutAsync(
            ct => ImportStreamAsync(importer, orderId, path, ct), timeout, userToken);
    }

    private static async Task<int> WithTimeoutAsync(
        Func<CancellationToken, Task<int>> operation, TimeSpan timeout, CancellationToken userToken)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "Тайм-аут має бути більшим за нуль");

        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userToken, timeoutCts.Token);

        try
        {
            return await operation(linkedCts.Token);
        }
        catch (OperationCanceledException ex)
            when (timeoutCts.IsCancellationRequested && !userToken.IsCancellationRequested)
        {
            throw new ImportTimeoutException(timeout, ex);
        }
    }

    private int ApplyLines(Order order, IReadOnlyList<OrderLine> lines, CancellationToken ct)
    {
        int added = 0;
        foreach (OrderLine line in lines)
        {
            ct.ThrowIfCancellationRequested();
            if (TryAddLine(order, line))
                added++;
        }

        return added;
    }

    // Ідемпотентність імпорту: товар, який уже є в замовленні, не дублюється.
    private bool TryAddLine(Order order, OrderLine line)
    {
        if (order.Lines.Any(l => string.Equals(l.ProductId, line.ProductId, StringComparison.OrdinalIgnoreCase)))
            return false;

        order.AddLine(line.ProductId, line.Name, line.Price, line.Quantity);
        _store.Update(order);
        OnChanged(order, OrderChangeKind.LineAdded);
        return true;
    }

    private Order RequireImportableOrder(string orderId)
    {
        Order order = Require(orderId);

        if (order.Status != OrderStatus.Draft)
            throw new DomainException(order.Status == OrderStatus.Confirmed
                ? $"Замовлення {order.Id} уже підтверджено: імпорт рядків заборонено"
                : $"Замовлення {order.Id} скасовано: імпорт рядків заборонено");

        return order;
    }

    private void OnChanged(Order order, OrderChangeKind kind)
    {
        OrderChangedEventArgs args = new(order.Id, kind, order.Lines.Count, order.Total);

        Changed?.Invoke(this, args);

        _notificationSink?.Notify(args);
    }

    private Order Require(string orderId) =>
        _store.GetById(orderId)
        ?? throw new DomainException($"Немає замовлення з id={orderId}.");
}
