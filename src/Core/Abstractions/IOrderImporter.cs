using Core.Domain;

namespace Core.Abstractions;

public interface IOrderImporter
{
    Task<IReadOnlyList<OrderLine>> LoadAsync(string path, CancellationToken ct = default);

    IAsyncEnumerable<OrderLine> LoadStreamAsync(string path, CancellationToken ct = default);
}
