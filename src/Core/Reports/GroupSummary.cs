namespace Core.Reports;

// DoD 1-2: уніфікований DTO групи для звітів за статусом і за клієнтом.
public sealed record GroupSummary(string Key, int Count, decimal Total);
