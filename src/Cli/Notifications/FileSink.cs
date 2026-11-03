using Core.Events;
using Core.Notifications;

namespace Cli.Notifications;

/// <summary>
/// Додаткове завдання 1: файловий одержувач сповіщень.
/// Каталог створюється в конструкторі, щоб запис не падав із DirectoryNotFoundException.
/// </summary>
public sealed class FileSink(string path) : INotificationSink
{
    private readonly string _path = Path.GetFullPath(path);

    public FileSink(string path, bool reset) : this(path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        if (reset)
            File.WriteAllText(_path, string.Empty);
    }

    public void Notify(OrderChangedEventArgs change)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.AppendAllText(_path, OrderChangeText.Format(change) + Environment.NewLine);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Збій журналу не повинен валити бізнес-операцію.
            Console.Error.WriteLine($"[FileSink] не вдалося записати {_path}: {ex.Message}");
        }
    }
}
