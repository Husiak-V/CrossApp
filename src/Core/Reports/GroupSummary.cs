namespace Core.Reports;

// уніфікований DTO групи для звітів за статусом і за клієнтом.
public sealed record GroupSummary(string Key, int Count, decimal Total);
