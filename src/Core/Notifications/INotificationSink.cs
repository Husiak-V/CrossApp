using Core.Events;

namespace Core.Notifications;

/// <summary>
/// Додаткове завдання 1: порт сповіщень для ручного DI.
/// На відміну від події, сервіс знає про одного одержувача, переданого конструктором.
/// Реалізації живуть у Cli, тому Core не залежить від консолі чи файлової системи.
/// </summary>
public interface INotificationSink
{
    void Notify(OrderChangedEventArgs change);
}
