using System.Globalization;
using Cli;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Reports;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");
bool useFactory = args.Contains("--factory");
bool useCache = args.Contains("--cache");

string command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

if (args.Contains("--help") || command is "help" or "-h")
{
    PrintCommands();
    return 0;
}

if (command == "lab6")
    return Lab6Scenario.Run(args);

IOrderStore store = useFactory
    ? StoreFactory.Create(args)
    : useFile
        ? new FileOrderStore(DataPaths.OrdersFile())
        : new InMemoryOrderStore(SampleData.Orders());

if (useCache)
    store = new CachingOrderStore(store);

var service = new OrderService(store);

// DoD 2, 4: звіти отримують IEnumerable<Order>; порожній набір не є помилкою.
IReadOnlyList<Order> items = service.All();

switch (command)
{
    // DoD 5: підкоманди report-* лише викликають ReportService і виводять DTO.
    case "report-top":
        int topOrders = IntAfter(args, command, 5);
        PrintOrders($"TOP-{topOrders} ORDERS BY TOTAL", ReportService.TopOrders(items, topOrders));
        break;

    case "report-over":
        decimal minTotal = DecimalAfter(args, command, 1000m);
        PrintOrders($"ORDERS OVER {minTotal:F2}", ReportService.AboveTotal(items, minTotal));
        break;

    case "report-groups":
        PrintGroups("ORDERS BY STATUS", "STATUS", ReportService.ByStatus(items));
        break;

    case "report-clients":
        int topCustomers = IntAfter(args, command, 5);
        PrintGroups($"TOP-{topCustomers} CLIENTS BY TOTAL", "CUSTOMER", ReportService.TopCustomers(items, topCustomers));
        break;

    case "report-sum":
        (int Orders, decimal Total, decimal Average, decimal Max) totals = ReportService.Totals(items);
        Console.WriteLine("TOTALS");
        Console.WriteLine($"{"ORDERS",6} {"TOTAL",12} {"AVERAGE",12} {"MAX",12}");
        Console.WriteLine($"{totals.Orders,6} {totals.Total,12:F2} {totals.Average,12:F2} {totals.Max,12:F2}");
        break;

    // DoD 6; Захист 4: два різні Count() до і після зміни джерела.
    case "demo-deferred":
        DeferredExecutionDemo.Run(items, DecimalAfter(args, command, 1000m));
        break;

    default:
        PrintCommands();
        return 1;
}

return 0;

static int IntAfter(string[] args, string command, int fallback)
{
    int index = Array.IndexOf(args, command);
    if (index < 0 || index + 1 >= args.Length)
        return fallback;

    return int.TryParse(args[index + 1], out int value) && value > 0 ? value : fallback;
}

static decimal DecimalAfter(string[] args, string command, decimal fallback)
{
    int index = Array.IndexOf(args, command);
    if (index < 0 || index + 1 >= args.Length)
        return fallback;

    return decimal.TryParse(args[index + 1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value)
        ? value
        : fallback;
}

static void PrintOrders(string title, IReadOnlyList<OrderReportLine> rows)
{
    Console.WriteLine(title);
    Console.WriteLine($"{"ID",-10} {"CUSTOMER",-12} {"STATUS",-11} {"TOTAL",12}");
    foreach (OrderReportLine row in rows)
        Console.WriteLine($"{row.OrderId,-10} {row.CustomerId,-12} {row.Status,-11} {row.Total,12:F2}");
}

static void PrintGroups(string title, string keyHeader, IReadOnlyList<GroupSummary> rows)
{
    Console.WriteLine(title);
    Console.WriteLine($"{keyHeader,-12} {"COUNT",6} {"TOTAL",12}");
    foreach (GroupSummary row in rows)
        Console.WriteLine($"{row.Key,-12} {row.Count,6} {row.Total,12:F2}");
}

static void PrintCommands()
{
    Console.WriteLine("report-top [n]");
    Console.WriteLine("report-over [minTotal]");
    Console.WriteLine("report-groups");
    Console.WriteLine("report-clients [n]");
    Console.WriteLine("report-sum");
    Console.WriteLine("demo-deferred [minTotal]");
    Console.WriteLine("lab6");
}
