using CatalogAPI.Data;
using CatalogAPI.Domain;
using CatalogAPI.Dtos;
using FCG.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Services;

public interface IPurchaseService
{
    Task<(bool ok, string? error, PurchaseResponse? response)> PlaceOrderAsync(Guid userId, PurchaseRequest request, CancellationToken ct);
    Task<IReadOnlyList<LibraryItemResponse>> GetLibraryAsync(Guid userId, CancellationToken ct);
}

public class PurchaseService(
    CatalogDbContext db,
    IPublishEndpoint publishEndpoint,
    ILogger<PurchaseService> logger) : IPurchaseService
{
    public async Task<(bool ok, string? error, PurchaseResponse? response)> PlaceOrderAsync(
        Guid userId, PurchaseRequest request, CancellationToken ct)
    {
        var game = await db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == request.GameId, ct);
        if (game is null)
            return (false, "Game not found.", null);

        var alreadyOwned = await db.LibraryItems.AnyAsync(l => l.UserId == userId && l.GameId == game.Id, ct);
        if (alreadyOwned)
            return (false, "User already owns this game.", null);

        var order = new Order
        {
            UserId = userId,
            GameId = game.Id,
            GameTitle = game.Title,
            Price = game.Price,
            Status = OrderStatus.Pending
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        // Start the purchase flow: PaymentsAPI will consume this.
        await publishEndpoint.Publish(new OrderPlacedEvent
        {
            OrderId = order.Id,
            UserId = order.UserId,
            GameId = order.GameId,
            GameTitle = order.GameTitle,
            Price = order.Price
        }, ct);

        logger.LogInformation("OrderPlacedEvent published for order {OrderId} (user {UserId}, game {GameId}).",
            order.Id, userId, game.Id);

        return (true, null, new PurchaseResponse
        {
            OrderId = order.Id,
            Status = order.Status.ToString(),
            Message = "Order placed. Payment is being processed."
        });
    }

    public async Task<IReadOnlyList<LibraryItemResponse>> GetLibraryAsync(Guid userId, CancellationToken ct) =>
        await db.LibraryItems.AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.AddedAt)
            .Select(l => new LibraryItemResponse
            {
                GameId = l.GameId,
                GameTitle = l.GameTitle,
                PricePaid = l.PricePaid,
                AddedAt = l.AddedAt
            })
            .ToListAsync(ct);
}
