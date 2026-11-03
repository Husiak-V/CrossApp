using Core.Events;
using Core.Notifications;

namespace Cli.Notifications;

/// <summary>
/// Додаткове завдання 1: композиція одержувачів.
/// Cli передає один INotificationSink у конструктор OrderService.
/// </summary>
public sealed class CompositeSink(params INotificationSink[] sinks) : INotificationSink
{
    private readonly INotificationSink[] _sinks = sinks ?? throw new ArgumentNullException(nameof(sinks));

    public void Notify(OrderChangedEventArgs change)
    {
        foreach (INotificationSink sink in _sinks)
            sink.Notify(change);
    }
}
