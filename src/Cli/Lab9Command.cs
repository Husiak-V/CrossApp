using Core;
using Core.Configuration;
using Core.Domain;
using Core.Services;
using Core.Storage;

namespace Cli;

internal static class Lab9Command
{
    public static int Run(
        string command,
        string[] args,
        AppOptions options,
        StorageOptions storage,
        string dataFile,
        string? apiKey)
    {
        // Лабораторна 9: команди працюють саме з файлом із конфігурації,
        // а не з legacy-шляхом --file часів лабораторної 5.
        var service = new OrderService(new FileOrderStore(dataFile));

        try
        {
            switch (command)
            {
                case "config":
                    PrintConfig(options, storage, dataFile, apiKey);
                    return 0;

                case "orders":
                    PrintOrders(service.All(), options.Orders.Currency);
                    return 0;

                // DoD 1: експорт усієї колекції у JSON за шляхом із конфігурації.
                case "export":
                    PrintHeader(dataFile, apiKey);
                    if (service.All().Count == 0)
                        service.ReplaceAll(SampleData.Orders());
                    string exportPath = ValueAfter(args, command) ?? dataFile;
                    Console.WriteLine($"Експортовано {service.ExportJson(exportPath)} замовлень.");
                    return 0;

                // DoD 1: імпорт замінює колекцію; пошкоджений orders.json теж можна відновити.
                case "import":
                    PrintHeader(dataFile, apiKey);
                    string importPath = ValueAfter(args, command)
                        ?? DataPaths.DataFile("orders-sample.json");
                    Console.WriteLine($"Імпортовано {service.ImportJson(importPath)} замовлень.");
                    return 0;

                default:
                    return 1;
            }
        }
        // Самоперевірка 4: технічні та доменні помилки — повідомлення без стектрейсу.
        catch (InvalidDataException ex)
        {
            Console.WriteLine($"Файл пошкоджено: {ex.Message}");
            return 4;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Файл не знайдено: {ex.FileName ?? ex.Message}");
            return 4;
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"Дані відхилено: {ex.Message}");
            return 3;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Дані відхилено: {ex.Message}");
            return 3;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Помилка читання/запису: {ex.Message}");
            return 4;
        }
    }

    private static void PrintHeader(string dataFile, string? apiKey)
    {
        Console.WriteLine($"Файл даних : {dataFile}");
        Console.WriteLine(apiKey is null
            ? "ApiKey: не заданий"
            : $"ApiKey: присутній, довжина {apiKey.Length}");
    }

    private static void PrintConfig(
        AppOptions options, StorageOptions storage, string dataFile, string? apiKey)
    {
        Console.WriteLine($"Середовище : {options.EnvironmentName}");
        Console.WriteLine($"DataPath   : {storage.DataPath}");
        Console.WriteLine($"FileName   : {storage.FileName}");
        Console.WriteLine($"Файл даних : {dataFile}");
        Console.WriteLine($"Logging    : {options.Logging.MinimumLevel}");
        Console.WriteLine($"Currency   : {options.Orders.Currency}");
        Console.WriteLine(apiKey is null
            ? "ApiKey     : не заданий"
            : $"ApiKey     : присутній, довжина {apiKey.Length}");
    }

    private static void PrintOrders(IReadOnlyList<Order> items, string currency)
    {
        if (items.Count == 0)
        {
            Console.WriteLine("Колекція порожня.");
            return;
        }

        Console.WriteLine($"{"ID",-10} {"CUSTOMER",-12} {"STATUS",-11} {"TOTAL",12}");
        foreach (Order order in items)
            Console.WriteLine(
                $"{order.Id,-10} {order.CustomerId,-12} {order.Status,-11} {order.Total,12:F2} {currency}");
    }

    private static string? ValueAfter(string[] args, string command, int offset = 1)
    {
        int index = Array.IndexOf(args, command);
        int target = index + offset;
        return index >= 0 && target < args.Length && !args[target].StartsWith("--")
            ? args[target]
            : null;
    }
}
