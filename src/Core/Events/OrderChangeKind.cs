namespace Core.Events;

/// <summary>
/// Вид успішної зміни стану замовлення, про яку повідомляють підписників.
/// </summary>
public enum OrderChangeKind
{
    OrderCreated,
    LineAdded,
    OrderConfirmed,
    OrderCancelled,
    OrderRemoved
}
