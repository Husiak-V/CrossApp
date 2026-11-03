using Core.Abstractions;
using Core.Domain;
using Core.Events;
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

    /// <summary>
    /// Доменна подія: підписники самі вирішують, як обробити зміну.
    /// Викликається лише після успішного проходження інваріантів.
    /// </summary>
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

    /// <summary>
    /// Пошук із критерієм, який задає викликач. Тут потрібен результат,
    /// тому використовується Func, а не подія.
    /// </summary>
    public IReadOnlyList<Order> FindAll(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    public IReadOnlyList<Order> Find(Func<Order, bool> predicate) => FindAll(predicate);

    private void OnChanged(Order order, OrderChangeKind kind)
    {
        OrderChangedEventArgs args = new(order.Id, kind, order.Lines.Count, order.Total);

        // Подія призначена для довільної кількості підписників.
        Changed?.Invoke(this, args);

        // Додаткове завдання 1: альтернативний шлях через ручний DI.
        _notificationSink?.Notify(args);
    }

    private Order Require(string orderId) =>
        _store.GetById(orderId)
        ?? throw new InvalidOperationException($"Немає замовлення з id={orderId}.");
}
