namespace Core.Events;

/// <summary>
/// Дані доменної події замовлення. Core лише описує, що сталося;
/// спосіб відображення чи запису визначає зовнішній код.
/// </summary>
public sealed class OrderChangedEventArgs(
    string orderId, OrderChangeKind kind, int linesCount, decimal total) : EventArgs
{
    public string OrderId { get; } = orderId;
    public OrderChangeKind Kind { get; } = kind;
    public int LinesCount { get; } = linesCount;
    public decimal Total { get; } = total;
    public DateTimeOffset At { get; } = DateTimeOffset.Now;

    public override string ToString() =>
        $"{Kind} {OrderId}: позицій {LinesCount}, сума {Total:F2} грн";
}
