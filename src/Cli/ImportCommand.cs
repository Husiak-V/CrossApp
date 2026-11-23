using System.Runtime.CompilerServices;
using System.Text.Json;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Import;
using Core.Services;

namespace Cli;

internal static class ImportCommand
{
    public static async Task<int> RunAsync(
        OrderService service, string[] args, string command, bool stream)
    {
        string? orderId = ValueAfter(args, command, 1);
        if (string.IsNullOrWhiteSpace(orderId))
        {
            Console.WriteLine($"{command} <orderId> [path] [--timeout seconds] [--slow]");
            return 1;
        }

        string path = ValueAfter(args, command, 2) ?? DataPaths.DataFile("order-lines.json");
        int timeoutSeconds = IntFlag(args, "--timeout");

        IOrderImporter importer = new JsonOrderImporter();
        if (args.Contains("--slow"))
            importer = new SlowOrderImporter(importer, TimeSpan.FromMilliseconds(300));

        using var cts = new CancellationTokenSource();
        ConsoleCancelEventHandler onCancel = (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        Console.CancelKeyPress += onCancel;

        try
        {
            int added = timeoutSeconds > 0
                ? stream
                    ? await service.ImportStreamAsync(importer, orderId, path, TimeSpan.FromSeconds(timeoutSeconds), cts.Token)
                    : await service.ImportAsync(importer, orderId, path, TimeSpan.FromSeconds(timeoutSeconds), cts.Token)
                : stream
                    ? await service.ImportStreamAsync(importer, orderId, path, cts.Token)
                    : await service.ImportAsync(importer, orderId, path, cts.Token);

            Console.WriteLine($"Імпортовано рядків: {added}");
            return 0;
        }
        // DoD 4; захист 3: тайм-аут і Ctrl+C — повідомлення, а не стектрейс.
        catch (ImportTimeoutException ex)
        {
            Console.WriteLine($"Імпорт перервано тайм-аутом {ex.Timeout.TotalSeconds:0.###} с.");
            return 2;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Імпорт скасовано.");
            return 2;
        }
        // DoD 5; захист 4: погані дані — доменна помилка з номером запису.
        catch (DomainException ex)
        {
            Console.WriteLine($"Дані відхилено: {ex.Message}");
            return 3;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Файл не знайдено: {path}");
            return 4;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Файл пошкоджено: {ex.Message}");
            return 4;
        }
        finally
        {
            Console.CancelKeyPress -= onCancel;
        }
    }

    private static string? ValueAfter(string[] args, string command, int offset)
    {
        int index = Array.IndexOf(args, command);
        int target = index + offset;
        return index >= 0 && target < args.Length && !args[target].StartsWith("--")
            ? args[target]
            : null;
    }

    private static int IntFlag(string[] args, string flag)
    {
        int index = Array.IndexOf(args, flag);
        return index >= 0 && index + 1 < args.Length
               && int.TryParse(args[index + 1], out int value) && value > 0
            ? value
            : 0;
    }
}

// Лише для демонстрації тайм-ауту в Cli: реальний JsonOrderImporter працює надто швидко.
internal sealed class SlowOrderImporter(IOrderImporter inner, TimeSpan delay) : IOrderImporter
{
    public async Task<IReadOnlyList<OrderLine>> LoadAsync(string path, CancellationToken ct = default)
    {
        IReadOnlyList<OrderLine> lines = await inner.LoadAsync(path, ct);
        for (int i = 0; i < lines.Count; i++)
            await Task.Delay(delay, ct);

        return lines;
    }

    public async IAsyncEnumerable<OrderLine> LoadStreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (OrderLine line in inner.LoadStreamAsync(path, ct).WithCancellation(ct))
        {
            await Task.Delay(delay, ct);
            yield return line;
        }
    }
}
