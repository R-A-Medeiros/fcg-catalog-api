namespace CatalogAPI.Domain;

// A game owned by a user (added after a payment is approved).
public class LibraryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public decimal PricePaid { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
