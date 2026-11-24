using Core.Configuration;
using Core.Domain;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public sealed class Lab9Tests
{
    // Завдання 1, 7; захист 1-2: шлях будується від явного базового каталогу.
    [Fact]
    public void StorageOptions_ResolveFullPath_IsAbsolute_AndCreatesDirectory()
    {
        string baseDir = TempDir();
        try
        {
            var options = new StorageOptions { DataPath = "data", FileName = "orders.json" };
            string path = options.ResolveFullPath(baseDir);

            Assert.True(Path.IsPathRooted(path));
            Assert.Equal(Path.Combine(baseDir, "data", "orders.json"), path);
            Assert.True(Directory.Exists(Path.Combine(baseDir, "data")));
        }
        finally
        {
            Directory.Delete(baseDir, recursive: true);
        }
    }

    // DoD 1; захист 8-9: camelCase, кирилиця без \uXXXX, .tmp не залишається.
    [Fact]
    public void FileOrderStore_Add_WritesReadableJsonAtomically()
    {
        string dir = TempDir();
        try
        {
            string path = Path.Combine(dir, "orders.json");
            var store = new FileOrderStore(path);

            Order order = Order.Create("ORD-1", "CUST-1");
            order.AddLine("P-1", "Цемент М400", 150m, 2);
            store.Add(order);

            string json = File.ReadAllText(path);
            Assert.Contains("\"customerId\": \"CUST-1\"", json);
            Assert.Contains("Цемент М400", json);
            Assert.DoesNotContain("\\u0426", json);
            Assert.False(File.Exists(path + ".tmp"));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    // Захист 7: PropertyNameCaseInsensitive дозволяє читати і PascalCase, і camelCase.
    [Fact]
    public void FileOrderStore_LoadsExistingJson_AndChecksInvariants()
    {
        string dir = TempDir();
        try
        {
            string path = Path.Combine(dir, "orders.json");
            File.WriteAllText(path, """
            [
              {
                "Id": "ORD-A",
                "CustomerId": "CUST-1",
                "Status": "Draft",
                "Lines": [
                  { "ProductId": "P-1", "Name": "Цемент", "Price": 100, "Quantity": 2 }
                ]
              }
            ]
            """);

            var store = new FileOrderStore(path);
            Order order = Assert.Single(store.List());

            Assert.Equal("ORD-A", order.Id);
            Assert.Equal(200m, order.Total);
            Assert.Equal(OrderStatus.Draft, order.Status);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    // Самоперевірка 4: зіпсований JSON дає зрозуміле повідомлення, а не сирий stack trace.
    [Fact]
    public void FileOrderStore_WithBrokenJson_ThrowsInvalidDataException()
    {
        string dir = TempDir();
        try
        {
            string path = Path.Combine(dir, "orders.json");
            File.WriteAllText(path, "{ not a json");

            var store = new FileOrderStore(path);
            InvalidDataException ex = Assert.Throws<InvalidDataException>(() => store.List());

            Assert.Contains("некоректний JSON", ex.Message);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    // DoD 1, 4: експорт та імпорт усієї колекції зі збереженням статусів і рядків.
    [Fact]
    public void OrderService_ExportImport_RoundTrips()
    {
        string dir = TempDir();
        try
        {
            var service = new OrderService(
                new FileOrderStore(Path.Combine(dir, "orders.json")));

            Order draft = service.CreateOrder("CUST-1", "ORD-A");
            service.AddLine(draft.Id, "P-1", "Цемент М400", 100m, 2);
            service.Confirm(draft.Id);

            Order cancelled = service.CreateOrder("CUST-2", "ORD-B");
            service.AddLine(cancelled.Id, "P-2", "Пісок будівельний", 50m, 1);
            service.Cancel(cancelled.Id);

            string exportPath = Path.Combine(dir, "export.json");
            Assert.Equal(2, service.ExportJson(exportPath));

            var restored = new OrderService(
                new FileOrderStore(Path.Combine(dir, "restored.json")));
            Assert.Equal(2, restored.ImportJson(exportPath));

            Assert.Equal(2, restored.All().Count);

            Order? confirmed = restored.Find("ORD-A");
            Assert.NotNull(confirmed);
            Assert.Equal(OrderStatus.Confirmed, confirmed.Status);
            Assert.Single(confirmed.Lines);
            Assert.Equal(200m, confirmed.Total);

            Order? cancelledRestored = restored.Find("ORD-B");
            Assert.NotNull(cancelledRestored);
            Assert.Equal(OrderStatus.Cancelled, cancelledRestored.Status);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    // DoD 1; захист 4: некоректний запис не частково замінює колекцію.
    [Fact]
    public void OrderService_Import_WithBrokenRecord_KeepsExistingCollection()
    {
        string dir = TempDir();
        try
        {
            var service = new OrderService(
                new FileOrderStore(Path.Combine(dir, "orders.json")));
            Order existing = service.CreateOrder("CUST-1", "ORD-A");
            service.AddLine(existing.Id, "P-1", "Товар A", 10m, 1);

            string importPath = Path.Combine(dir, "bad.json");
            File.WriteAllText(importPath, """
            [
              {
                "id": "ORD-1",
                "customerId": "CUST-1",
                "status": "Draft",
                "lines": [
                  { "productId": "P-1", "name": "Товар A", "price": 10, "quantity": 1 }
                ]
              },
              {
                "id": "ORD-2",
                "customerId": "CUST-2",
                "status": "Draft",
                "lines": [
                  { "productId": "P-2", "name": "Товар B", "price": 10, "quantity": 0 }
                ]
              }
            ]
            """);

            InvalidDataException ex = Assert.Throws<InvalidDataException>(
                () => service.ImportJson(importPath));

            Assert.Contains("Запис 2", ex.Message);
            Order kept = Assert.Single(service.All());
            Assert.Equal("ORD-A", kept.Id);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    // Захист 4: порушення доменного інваріанта під час імпорту теж стає InvalidDataException.
    [Fact]
    public void OrderService_Import_WithEmptyConfirmedOrder_ThrowsInvalidDataException()
    {
        string dir = TempDir();
        try
        {
            var service = new OrderService(
                new FileOrderStore(Path.Combine(dir, "orders.json")));

            string importPath = Path.Combine(dir, "bad.json");
            File.WriteAllText(importPath, """
            [
              { "id": "ORD-EMPTY", "customerId": "CUST-1", "status": "Confirmed", "lines": [] }
            ]
            """);

            InvalidDataException ex = Assert.Throws<InvalidDataException>(
                () => service.ImportJson(importPath));

            Assert.Contains("Запис 1", ex.Message);
            Assert.Empty(service.All());
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    private static string TempDir()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"crossapp-lab9-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        return dir;
    }
}
