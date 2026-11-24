namespace Core.Configuration;

// Лабораторна 9 (DoD 3): POCO для config.Get<AppOptions>(); Core не знає про JSON і user-secrets.
public sealed class AppOptions
{
    public StorageOptions Storage { get; init; } = new();
    public OrdersOptions Orders { get; init; } = new();
    public LoggingOptions Logging { get; init; } = new();
    public string EnvironmentName { get; init; } = "Development";
}

public sealed class StorageOptions
{
    public string DataPath { get; init; } = "data";
    public string FileName { get; init; } = "orders.json";

    // Захист 1-2: явний базовий каталог + Path.Combine замість ручного склеювання рядків.
    public string ResolveFullPath(string baseDir)
    {
        string root = Path.IsPathRooted(DataPath) ? DataPath : Path.Combine(baseDir, DataPath);
        Directory.CreateDirectory(root);
        return Path.GetFullPath(Path.Combine(root, FileName));
    }
}

public sealed class OrdersOptions
{
    public string Currency { get; init; } = "грн";
}

public sealed class LoggingOptions
{
    public string MinimumLevel { get; init; } = "Information";
}
