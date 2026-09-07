using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var appInfo = new
{
    Project = "CrossApp — практикум з крос-платформного програмування",
    Student = new
    {
        FullName = "Гусяк Володимир", 
        Group = "ФЕІ-33"          
    },
    Environment = new
    {
        OsDescription = RuntimeInformation.OSDescription,
        OsVersion = Environment.OSVersion.ToString(),
        ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
        ClrVersion = Environment.Version.ToString(),
        RuntimeFramework = RuntimeInformation.FrameworkDescription,
        AppBaseDirectory = AppContext.BaseDirectory,
        CurrentDirectory = Environment.CurrentDirectory
    },
    Domain = new
    {
        Name = "Замовлення (Orders)",
        Description = "Оформлення замовлень, розрахунок загальної вартості та облік позицій товарів",
        Entities = new[]
        {
            new { Name = "Customer", Purpose = "Клієнт або замовник (контактні дані, статус)" },
            new { Name = "Product", Purpose = "Товар каталогу (артикул, назва, ціна за одиницю)" },
            new { Name = "Order", Purpose = "Замовлення (дата, статус, зв'язок із клієнтом, фінальна сума)" },
            new { Name = "OrderLine", Purpose = "Рядок/позиція замовлення (конкретний товар, кількість, ціна фіксації)" }
        }
    }
};

bool isJsonMode = args.Contains("--json", StringComparer.OrdinalIgnoreCase);

if (isJsonMode)
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
    };
    string jsonOutput = JsonSerializer.Serialize(appInfo, options);
    Console.WriteLine(jsonOutput);
}
else
{
    Console.WriteLine(appInfo.Project);
    Console.WriteLine($"Студент: {appInfo.Student.FullName}, група {appInfo.Student.Group}");
    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"ОС (OSDescription)  : {appInfo.Environment.OsDescription}");
    Console.WriteLine($"ОС (Environment)    : {appInfo.Environment.OsVersion}");
    Console.WriteLine($"Архітектура процесу : {appInfo.Environment.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR)   : {appInfo.Environment.ClrVersion}");
    Console.WriteLine($"Runtime             : {appInfo.Environment.RuntimeFramework}");
    Console.WriteLine($"Каталог застосунку  : {appInfo.Environment.AppBaseDirectory}");
    Console.WriteLine($"Поточний каталог    : {appInfo.Environment.CurrentDirectory}");
    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"Предметна область   : {appInfo.Domain.Name}");
    Console.WriteLine($"Призначення         : {appInfo.Domain.Description}");
    Console.WriteLine("Ключові сутності    : Customer, Product, Order, OrderLine");
    Console.WriteLine(new string('-', 60));
}