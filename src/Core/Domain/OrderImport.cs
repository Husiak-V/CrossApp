using Core.Dto;

namespace Core.Domain;

public static class OrderImport
{
    public static ImportResult<OrderLine> ToEntities(
        ImportResult<OrderLineDto> import, Action<string>? onSkipped = null)
    {
        ArgumentNullException.ThrowIfNull(import);

        var entities = new List<OrderLine>();
        var errors = new List<string>(import.Errors);

        for (int i = 0; i < import.Items.Count; i++)
        {
            OrderLineDto dto = import.Items[i];
            try
            {
                entities.Add(OrderLine.FromDto(dto)); 
            }
            catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
            {
                string error = $"рядок даних {i + 1} (товар '{dto?.ProductId}'): {ex.Message}";
                errors.Add(error);
                onSkipped?.Invoke(error);
            }
        }

        return new ImportResult<OrderLine>(entities, errors);
    }

    public static (Order Order, IReadOnlyList<string> Errors) DraftFromImport(
        string orderId, string customerId, ImportResult<OrderLineDto> import,
        Action<string>? onSkipped = null)
    {
        Order order = Order.Create(orderId, customerId);
        ImportResult<OrderLine> lines = ToEntities(import, onSkipped);

        var errors = new List<string>(lines.Errors);
        foreach (OrderLine line in lines.Items)
            order.AddLine(line.ProductId, line.Name, line.Price, line.Quantity);

        return (order, errors);
    }
}
