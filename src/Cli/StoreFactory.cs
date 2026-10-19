using Core;
using Core.Abstractions;
using Core.Storage;

internal static class StoreFactory
{
    public static IOrderStore Create(string[] args)
    {
        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "orders.json");
        return (args ?? []).Contains("--file")
            ? new FileOrderStore(dataPath)
            : new InMemoryOrderStore(SampleData.Orders());
    }
}
