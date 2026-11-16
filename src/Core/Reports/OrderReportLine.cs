namespace Core.Reports;

// DoD 1, 5: рядок звіту є DTO, а не доменною сутністю Order.
public sealed record OrderReportLine(
    string OrderId,
    string CustomerId,
    string Status,
    decimal Total);
