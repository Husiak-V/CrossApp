using Core;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public sealed class OrderServiceTests
{
    // DoD 6: інваріант — відмова не змінює стан замовлення.
    [Fact]
    public void AddLine_WithNegativeQuantity_Throws_AndKeepsState()
    {
        var store = new InMemoryOrderStore();
        var service = new OrderService(store);
        Order order = service.CreateOrder("CUST-1", "ORD-T1");
        service.AddLine(order.Id, "P-1", "Кабель", 10m, 5);
        decimal before = store.GetById(order.Id)!.Total;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            service.AddLine(order.Id, "P-2", "Гвинт", 10m, -5));

        Assert.Equal(before, store.GetById(order.Id)!.Total);
        Assert.Single(store.GetById(order.Id)!.Lines);
    }

    // DoD 6: подія піднімається рівно один раз після успішної зміни.
    [Fact]
    public void AddLine_RaisesChangedExactlyOnce()
    {
        var service = new OrderService(new InMemoryOrderStore());
        Order order = service.CreateOrder("CUST-1", "ORD-T2");
        var events = new List<OrderChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        service.AddLine(order.Id, "P-1", "Кабель", 10m, 2);

        Assert.Single(events);
        Assert.Equal(OrderChangeKind.LineAdded, events[0].Kind);
        Assert.Equal(20m, events[0].Total);
    }

    // DoD 5: бізнес-правило — підтверджене замовлення не приймає рядки.
    [Fact]
    public void AddLine_WhenOrderConfirmed_ThrowsDomainException()
    {
        Order order = Order.Create("ORD-T3", "CUST-1");
        order.AddLine("P-1", "Кабель", 10m, 1);
        order.Confirm();

        DomainException ex = Assert.Throws<DomainException>(() =>
            order.AddLine("P-2", "Гвинт", 5m, 1));

        Assert.Contains("уже підтверджено", ex.Message);
    }
}
