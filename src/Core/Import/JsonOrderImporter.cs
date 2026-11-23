using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Import;

public sealed class JsonOrderImporter : IOrderImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // DoD 2, 3; захист 2: справжній async-ввід через FileStream(useAsync: true),
    // ct передається в кожен await і перевіряється в циклі.
    public async Task<IReadOnlyList<OrderLine>> LoadAsync(string path, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

        List<OrderLineDto> dtos =
            await JsonSerializer.DeserializeAsync<List<OrderLineDto>>(stream, Options, ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку рядків.");

        var result = new List<OrderLine>(dtos.Count);
        for (int i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            result.Add(ToLine(dtos[i], i));
        }

        return result;
    }

    // Додаткове завдання 1: потокове читання JSON без матеріалізації всього списку.
    // [EnumeratorCancellation] дозволяє передати токен через await foreach ... WithCancellation.
    public async IAsyncEnumerable<OrderLine> LoadStreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

        int number = 0;
        await foreach (OrderLineDto? dto in JsonSerializer
            .DeserializeAsyncEnumerable<OrderLineDto>(stream, Options, ct)
            .WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            number++;
            yield return ToLine(dto, number - 1);
        }
    }

    // Погані дані — доменна помилка з номером запису; ArgumentException лишається InnerException.
    private static OrderLine ToLine(OrderLineDto? dto, int index)
    {
        if (dto is null)
            throw new DomainException($"Запис {index + 1}: порожній запис.");

        try
        {
            return OrderLine.FromDto(dto);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
        {
            throw new DomainException($"Запис {index + 1}: {ex.Message}", ex);
        }
    }
}
