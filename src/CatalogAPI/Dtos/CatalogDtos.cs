using System.ComponentModel.DataAnnotations;

namespace CatalogAPI.Dtos;

public record GameRequest
{
    [Required, MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Range(0, 100000)]
    public decimal Price { get; init; }

    [MaxLength(100)]
    public string Genre { get; init; } = string.Empty;
}

public record GameResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Genre { get; init; } = string.Empty;
}

public record PurchaseRequest
{
    [Required]
    public Guid GameId { get; init; }
}

public record PurchaseResponse
{
    public Guid OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public record LibraryItemResponse
{
    public Guid GameId { get; init; }
    public string GameTitle { get; init; } = string.Empty;
    public decimal PricePaid { get; init; }
    public DateTime AddedAt { get; init; }
}
