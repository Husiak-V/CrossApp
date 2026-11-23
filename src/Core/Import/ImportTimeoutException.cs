namespace Core.Import;

/// <summary>
/// Окремий вид скасування: спрацював тайм-аут, а не токен користувача
/// (додаткове завдання 2). Успадковує OperationCanceledException, тому
/// загальний catch скасування продовжує працювати.
/// </summary>
public sealed class ImportTimeoutException : OperationCanceledException
{
    public ImportTimeoutException(TimeSpan timeout, Exception? inner = null)
        : base($"Імпорт перервано тайм-аутом {timeout.TotalSeconds:0.###} с.", inner)
    {
        Timeout = timeout;
    }

    public TimeSpan Timeout { get; }
}
