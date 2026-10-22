namespace CatalogAPI.Domain;

public enum OrderStatus
{
    Pending,
    Approved,
    Rejected
}

// Tracks a purchase between OrderPlacedEvent and PaymentProcessedEvent.
public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
