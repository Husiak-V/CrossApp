using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileOrderStore(string path) : IOrderStore
{
    private readonly Dictionary<string, Order> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (!File.Exists(_path))
        {
            _loaded = true;
            return;
        }

        foreach (OrderDto dto in OrderJson.Read(_path))
        {
            Order order = Order.FromDto(dto); // повторно перевіряє інваріанти
            _cache[order.Id] = order;
        }

        _loaded = true;
    }

    // Захист 9: атомарний запис — спочатку .tmp, потім File.Move.
    private void Flush() =>
        OrderJson.WriteAtomic(_path, _cache.Values.Select(order => order.ToDto()));

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
        EnsureLoaded();
        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Замовлення з id={item.Id} уже існує.");
        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;
        Flush();
        return true;
    }

    // Імпорт замінює колекцію цілком; старий файл не читається,
    // тому пошкоджений orders.json можна відновити командою import.
    public void ReplaceAll(IEnumerable<Order> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _cache.Clear();
        foreach (Order item in items)
        {
            ArgumentNullException.ThrowIfNull(item);
            _cache[item.Id] = item;
        }

        _loaded = true;
        Flush();
    }
}
