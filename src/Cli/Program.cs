using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

string extension = Path.GetExtension(path).ToLowerInvariant();

ImportResult<ProductDto> result = extension switch
{
    ".json" => ProductJsonImporter.Load(path),
    _ => ProductCsvImporter.Load(path)
};

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine($"  {p.Id,-9} {p.Name,-26} {p.Price,8:F2} грн  {p.Unit,-4} {(p.Category != null ? p.Category : string.Empty)}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($"  ! {e}");
    }
}

// Додаткове завдання 3: Статистика імпорту одним рядком
int total = result.Items.Count + result.Errors.Count;
int accepted = result.Items.Count;
int skipped = result.Errors.Count;
double errorPercent = total > 0 ? (double)skipped / total * 100 : 0.0;
Console.WriteLine($"\nСтатистика імпорту: Усього: {total} | Прийнято: {accepted} | Пропущено: {skipped} | Помилок: {errorPercent:F1}%");

return 0;
