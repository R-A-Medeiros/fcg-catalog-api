using CatalogAPI.Data;
using CatalogAPI.Domain;
using FCG.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Consumers;

// Consumes PaymentProcessedEvent. If approved, adds the game to the user's library.
public class PaymentProcessedConsumer(
    CatalogDbContext db,
    ILogger<PaymentProcessedConsumer> logger) : IConsumer<PaymentProcessedEvent>
{
    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var msg = context.Message;
        var ct = context.CancellationToken;

        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == msg.OrderId, ct);
        if (order is null)
        {
            logger.LogWarning("PaymentProcessedEvent for unknown order {OrderId}. Ignoring.", msg.OrderId);
            return;
        }

        if (order.Status != OrderStatus.Pending)
        {
            logger.LogInformation("Order {OrderId} already resolved as {Status}. Ignoring duplicate.", order.Id, order.Status);
            return;
        }

        order.ResolvedAt = DateTime.UtcNow;

        if (msg.Status == PaymentStatus.Approved)
        {
            order.Status = OrderStatus.Approved;

            var alreadyOwned = await db.LibraryItems.AnyAsync(
                l => l.UserId == msg.UserId && l.GameId == msg.GameId, ct);

            if (!alreadyOwned)
            {
                db.LibraryItems.Add(new LibraryItem
                {
                    UserId = msg.UserId,
                    GameId = msg.GameId,
                    GameTitle = msg.GameTitle,
                    PricePaid = msg.Price
                });
            }

            logger.LogInformation("Payment approved. Game {GameId} added to library of user {UserId}.",
                msg.GameId, msg.UserId);
        }
        else
        {
            order.Status = OrderStatus.Rejected;
            logger.LogInformation("Payment rejected for order {OrderId}. Game not added.", order.Id);
        }

        await db.SaveChangesAsync(ct);
    }
}
