using System.Security.Claims;
using CatalogAPI.Dtos;
using CatalogAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class PurchasesController(IPurchaseService purchaseService) : ControllerBase
{
    /// <summary>Places an order for a game, starting the purchase flow (publishes OrderPlacedEvent).</summary>
    [HttpPost("purchases")]
    public async Task<IActionResult> Purchase([FromBody] PurchaseRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { error = "Invalid token: missing user id." });

        var (ok, error, response) = await purchaseService.PlaceOrderAsync(userId, request, ct);
        return ok ? Accepted(response) : BadRequest(new { error });
    }

    /// <summary>Returns the authenticated user's game library.</summary>
    [HttpGet("library")]
    public async Task<IActionResult> Library(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { error = "Invalid token: missing user id." });

        return Ok(await purchaseService.GetLibraryAsync(userId, ct));
    }

    private bool TryGetUserId(out Guid userId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out userId);
    }
}
