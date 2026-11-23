using System.Runtime.CompilerServices;
using Core.Abstractions;
using Core.Domain;

namespace Core.Tests;

internal sealed class StubOrderImporter(IReadOnlyList<OrderLine> lines) : IOrderImporter
{
    public Task<IReadOnlyList<OrderLine>> LoadAsync(string path, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(lines);
    }

    public async IAsyncEnumerable<OrderLine> LoadStreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (OrderLine line in lines)
        {
            ct.ThrowIfCancellationRequested();
            yield return line;
            await Task.Yield();
        }
    }
}

internal sealed class BlockingOrderImporter : IOrderImporter
{
    private readonly TaskCompletionSource _started =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Started => _started.Task;

    public async Task<IReadOnlyList<OrderLine>> LoadAsync(string path, CancellationToken ct = default)
    {
        _started.TrySetResult();
        await Task.Delay(Timeout.InfiniteTimeSpan, ct);
        return [];
    }

    public async IAsyncEnumerable<OrderLine> LoadStreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        _started.TrySetResult();
        await Task.Delay(Timeout.InfiniteTimeSpan, ct);
        yield break;
    }
}

internal sealed class CancellingOrderImporter(CancellationTokenSource cts) : IOrderImporter
{
    public Task<IReadOnlyList<OrderLine>> LoadAsync(string path, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<OrderLine>>(
        [
            OrderLine.Create("P-1", "Перший", 10m, 1),
            OrderLine.Create("P-2", "Другий", 10m, 1)
        ]);

    public async IAsyncEnumerable<OrderLine> LoadStreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        yield return OrderLine.Create("P-1", "Перший", 10m, 1);

        cts.Cancel();
        ct.ThrowIfCancellationRequested();

        yield return OrderLine.Create("P-2", "Другий", 10m, 1);
        await Task.Yield();
    }
}
