using Core.Events;

namespace Cli.Notifications;

/// <summary>
/// Єдиний формат рядка журналу для консольного обробника події та FileSink.
/// </summary>
internal static class OrderChangeText
{
    public static string Format(OrderChangedEventArgs e) =>
        $"{e.At:HH:mm:ss} {e.Kind,-14} {e.OrderId} -> позицій: {e.LinesCount}, сума: {e.Total:F2} грн";
}
