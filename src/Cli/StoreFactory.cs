using Core;
using Core.Abstractions;
using Core.Storage;

internal static class StoreFactory
{
    public static IOrderStore Create(string[] args)
    {
        string dataPath = DataPaths.OrdersFile();
        return (args ?? []).Contains("--file")
            ? new FileOrderStore(dataPath)
            : new InMemoryOrderStore(SampleData.Orders());
    }
}
