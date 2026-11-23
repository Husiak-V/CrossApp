namespace Core.Reports;

// рядок звіту є DTO, а не доменною сутністю Order.
public sealed record OrderReportLine(
    string OrderId,
    string CustomerId,
    string Status,
    decimal Total);
