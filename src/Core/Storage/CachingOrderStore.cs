using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingOrderStore(IOrderStore inner) : IOrderStore
{
    private readonly IOrderStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly Dictionary<string, Order> _cache = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;

    private void EnsureLoaded()
    {
        if (_loaded) return;
        foreach (Order order in _inner.List())
            _cache[order.Id] = order;
        _loaded = true;
    }

    public IReadOnlyList<Order> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Order? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _inner.Add(item);
        _cache[item.Id] = item;
    }

    public void Update(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _inner.Update(item);
        _cache[item.Id] = item;
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);
        if (removed) _cache.Remove(id);
        return removed;
    }

    // Лабораторна 9 (DoD 1): спочатку замінюємо джерело, потім перебудовуємо кеш.
    public void ReplaceAll(IEnumerable<Order> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _inner.ReplaceAll(items);
        _cache.Clear();
        foreach (Order order in _inner.List())
            _cache[order.Id] = order;

        _loaded = true;
    }
}
