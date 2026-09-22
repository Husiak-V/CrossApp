namespace Core.Dto;

public record ProductDto(
    string Id,
    string Name,
    decimal Price,
    string Unit,
    string? Category = null);
