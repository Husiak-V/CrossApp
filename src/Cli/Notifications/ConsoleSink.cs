using Core.Events;
using Core.Notifications;

namespace Cli.Notifications;

/// <summary>
/// Додаткове завдання 1: одна з реалізацій INotificationSink.
/// Форматування живе в Cli, тому Core не знає про консоль.
/// </summary>
public sealed class ConsoleSink : INotificationSink
{
    public void Notify(OrderChangedEventArgs change) =>
        Console.WriteLine($"[sink] {OrderChangeText.Format(change)}");
}
