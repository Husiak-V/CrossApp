using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Dto;

namespace Core.Storage;

internal static class OrderJson
{
    // Захист 7-8: camelCase у файлі, кирилиця без escape-послідовностей Юнікоду,
    // читання без залежності від регістру.
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static List<OrderDto> Read(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<List<OrderDto>>(File.ReadAllText(path), Options) ?? [];
        }
        // Самоперевірка 4: зіпсований JSON перетворюється на зрозуміле повідомлення.
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Файл '{path}' містить некоректний JSON: {ex.Message}", ex);
        }
    }

    public static string Serialize(IEnumerable<OrderDto> items) =>
        JsonSerializer.Serialize(items.ToList(), Options);

    // Захист 9: спочатку .tmp, потім File.Move — читач ніколи не бачить напівзаписаний файл.
    public static void WriteAtomic(string path, IEnumerable<OrderDto> items)
    {
        string fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        string tmp = fullPath + ".tmp";
        File.WriteAllText(tmp, Serialize(items));
        File.Move(tmp, fullPath, overwrite: true);
    }
}
