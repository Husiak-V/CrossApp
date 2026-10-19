using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class OrderService(IOrderStore store)
{
    private readonly IOrderStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public Order CreateOrder(string customerId, string? orderId = null)
    {
        OrderPolicy.EnsureCanCreateOrder(_store.List(), customerId);

        string id = string.IsNullOrWhiteSpace(orderId)
            ? $"ORD-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}"
            : orderId;

        Order order = Order.Create(id, customerId);
        _store.Add(order);
        return order;
    }

    public void AddLine(string orderId, string productId, string name, decimal price, int quantity)
    {
        Order order = Require(orderId);
        order.AddLine(productId, name, price, quantity);
        _store.Update(order);
    }

    public void Confirm(string orderId)
    {
        Order order = Require(orderId);
        order.Confirm();
        _store.Update(order);
    }

    public void Cancel(string orderId)
    {
        Order order = Require(orderId);
        order.Cancel();
        _store.Update(order);
    }

    public bool Remove(string orderId) => _store.Remove(orderId);

    public IReadOnlyList<Order> All() => _store.List();

    public Order? Find(string id) => _store.GetById(id);

    public IReadOnlyList<Order> Find(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    private Order Require(string orderId) =>
        _store.GetById(orderId)
        ?? throw new InvalidOperationException($"Немає замовлення з id={orderId}.");
}
