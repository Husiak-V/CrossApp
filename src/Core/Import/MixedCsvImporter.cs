using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var products = new List<ProductDto>();
        var customers = new List<CustomerDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (number == 1 && line.StartsWith("type", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(line))
            {
                case MixedOutcome.ProductOk p:
                    products.Add(p.Value);
                    break;
                case MixedOutcome.CustomerOk c:
                    customers.Add(c.Value);
                    break;
                case MixedOutcome.Failed f:
                    errors.Add($"рядок {number}: {f.Reason}");
                    break;
            }
        }

        return new MixedImportResult(products, customers, errors);
    }

    private static MixedOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 2 } => new MixedOutcome.Failed("недостатньо колонок для визначення типу"),

            ["P", var id, var name, var priceStr, var unit]
                when decimal.TryParse(priceStr, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) && price >= 0
                => new MixedOutcome.ProductOk(new ProductDto(id, name, price, unit)),

            ["P", _, "", _, _]
                => new MixedOutcome.Failed("назва товару порожня"),

            ["P", ..]
                => new MixedOutcome.Failed("помилка формату рядка товару (P)"),

            ["C", var id, var name, var email] when email.Contains('@')
                => new MixedOutcome.CustomerOk(new CustomerDto(id, name, email)),

            ["C", _, _, var email] when !email.Contains('@')
                => new MixedOutcome.Failed($"некоректний email клієнта: '{email}'"),

            ["C", ..]
                => new MixedOutcome.Failed("помилка формату рядка клієнта (C)"),

            [var type, ..] => new MixedOutcome.Failed($"невідомий тип запису: '{type}'")
        };
    }

    private abstract record MixedOutcome
    {
        public sealed record ProductOk(ProductDto Value) : MixedOutcome;
        public sealed record CustomerOk(CustomerDto Value) : MixedOutcome;
        public sealed record Failed(string Reason) : MixedOutcome;
    }
}
