namespace Core.Domain;

public static class OrderPolicy
{
    public const int MaxOpenOrdersPerCustomer = 5;

    public static void EnsureCanCreateOrder(
        IEnumerable<Order> existingOrders, string customerId, int maxOpen = MaxOpenOrdersPerCustomer)
    {
        ArgumentNullException.ThrowIfNull(existingOrders);
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Ідентифікатор клієнта обов'язковий", nameof(customerId));
        if (maxOpen <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxOpen), maxOpen, "Ліміт має бути більшим за нуль");

        int openCount = existingOrders.Count(o =>
            o.CustomerId == customerId.Trim() &&
            o.Status is OrderStatus.Draft or OrderStatus.Confirmed);

        if (openCount >= maxOpen)
            throw new DomainException(
                $"Клієнт {customerId.Trim()} уже має {openCount} відкритих замовлень (ліміт {maxOpen}): нове замовлення створити не можна");
    }
}
