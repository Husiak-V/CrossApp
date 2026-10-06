namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public bool IsConfirmed { get; private set; }
    public decimal Total => _lines.Sum(l => l.Price * l.Quantity);

    private Order(string id, string customerId)
    {
        Id = id;
        CustomerId = customerId;
    }

    public static Order Create(string id, string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор замовлення обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Ідентифікатор клієнта обов'язковий", nameof(customerId));

        return new Order(id.Trim(), customerId.Trim());
    }

    public void AddLine(string productId, string name, decimal price, int quantity)
    {
        if (IsConfirmed)
            throw new InvalidOperationException($"Замовлення {Id} вже підтверджене, рядки додавати не можна");
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Ідентифікатор товару обов'язковий", nameof(productId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва товару не може бути порожньою", nameof(name));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Ціна не може бути від'ємною");
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Кількість у рядку має бути більшою за нуль");

        _lines.Add(new OrderLine(productId.Trim(), name.Trim(), price, quantity));
    }

    public void Confirm()
    {
        if (IsConfirmed)
            throw new InvalidOperationException($"Замовлення {Id} вже підтверджене");
        if (_lines.Count == 0)
            throw new InvalidOperationException($"Не можна підтвердити порожнє замовлення {Id}");

        IsConfirmed = true;
    }

    public override string ToString() => $"Замовлення {Id} (Клієнт: {CustomerId}) — Позицій: {_lines.Count}, Сума: {Total:F2} грн, Статус: {(IsConfirmed ? "Підтверджено" : "Чернетка")}";
}
