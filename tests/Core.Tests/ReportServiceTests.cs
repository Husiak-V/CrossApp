using Core.Reports;

namespace Core.Tests;

public sealed class ReportServiceTests
{
    // DoD 6: [Theory] перевіряє кілька наборів даних одним тестом.
    [Theory]
    [InlineData(0, 4)]
    [InlineData(50, 2)]
    [InlineData(100, 1)]
    [InlineData(1000, 0)]
    public void AboveTotal_CountsOnlyOrdersAboveThreshold(decimal threshold, int expected)
    {
        Assert.Equal(expected, ReportService.AboveTotal(TestData.Orders(), threshold).Count);
    }

    // DoD 6: топ-N детермінований завдяки ThenBy(Id) навіть за однакових сум.
    [Fact]
    public void TopOrders_IsDeterministic()
    {
        IReadOnlyList<string> ids = ReportService
            .TopOrders(TestData.Orders(), 3)
            .Select(o => o.OrderId)
            .ToList();

        Assert.Equal(new[] { "ORD-A", "ORD-C", "ORD-B" }, ids);
    }

    // DoD 6: порожнє джерело не кидає InvalidOperationException.
    [Fact]
    public void Totals_OnEmptySource_ReturnsZeros()
    {
        (int Orders, decimal Total, decimal Average, decimal Max) totals =
            ReportService.Totals(Array.Empty<Core.Domain.Order>());

        Assert.Equal((0, 0m, 0m, 0m), totals);
    }
}
