using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    public bool IsConfirmed => Status == OrderStatus.Confirmed;
    public decimal Total => _lines.Sum(l => l.Price * l.Quantity);

    private Order(string id, string customerId, OrderStatus status)
    {
        Id = id;
        CustomerId = customerId;
        Status = status;
    }

    public static Order Create(string id, string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор замовлення обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Ідентифікатор клієнта обов'язковий", nameof(customerId));

        return new Order(id.Trim(), customerId.Trim(), OrderStatus.Draft);
    }

    public void AddLine(string productId, string name, decimal price, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException(
                $"Замовлення {Id} у статусі {Status}, рядки можна додавати лише в чернетці");

        // Перевірки ціни/кількості/назви — всередині OrderLine.Create.
        _lines.Add(OrderLine.Create(productId, name, price, quantity));
    }

    public void Confirm()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException($"Не можна підтвердити порожнє замовлення {Id}");

        TransitionTo(OrderStatus.Confirmed);
    }

    public void Cancel()
    {
        TransitionTo(OrderStatus.Cancelled);
    }

    private void TransitionTo(OrderStatus target)
    {
        bool allowed = (Status, target) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed) => true,
            (OrderStatus.Draft, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException(
                $"Замовлення {Id}: перехід зі статусу {Status} у {target} заборонено");

        Status = target;
    }

    public OrderDto ToDto() =>
        new(Id, CustomerId, Status.ToString(), _lines.Select(l => l.ToDto()).ToList());

    public static Order FromDto(OrderDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        Order order = Create(dto.Id, dto.CustomerId);

        foreach (OrderLineDto line in dto.Lines ?? [])
            order._lines.Add(OrderLine.FromDto(line)); // ті самі перевірки, що й Create

        if (Enum.TryParse<OrderStatus>(dto.Status, out OrderStatus status) && status != OrderStatus.Draft)
        {
            if (status == OrderStatus.Confirmed)
                order.Confirm(); // кине InvalidOperationException для порожнього
            else
                order.TransitionTo(status);
        }

        return order;
    }

    public override string ToString() => $"Замовлення {Id} (Клієнт: {CustomerId}) — Позицій: {_lines.Count}, Сума: {Total:F2} грн, Статус: {Status}";
}
