internal static class DataPaths
{
    public static string RepositoryRoot()
    {
        DirectoryInfo? dir = new(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "CrossApp.slnx")))
            dir = dir.Parent;

        return dir?.FullName ?? AppContext.BaseDirectory;
    }

    public static string OrdersFile() => Path.Combine(RepositoryRoot(), "data", "orders.json");

    public static string DataFile(string name) => Path.Combine(RepositoryRoot(), "data", name);

    /// <summary>
    /// Лабораторна 6: журнал доменних подій лежить у корені проєкту, поруч із data/.
    /// </summary>
    public static string LogFile() => Path.Combine(RepositoryRoot(), "logs", "app.log");

    public static string SinkLogFile() => Path.Combine(RepositoryRoot(), "logs", "sinks.log");
}
