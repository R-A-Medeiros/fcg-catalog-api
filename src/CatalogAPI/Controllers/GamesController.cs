using CatalogAPI.Dtos;
using CatalogAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController(IGameService gameService) : ControllerBase
{
    /// <summary>Lists all games. Public.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await gameService.GetAllAsync(ct));

    /// <summary>Gets a game by id. Public.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var game = await gameService.GetByIdAsync(id, ct);
        return game is null ? NotFound() : Ok(game);
    }

    /// <summary>Creates a game. Restricted to administrators.</summary>
    [HttpPost]
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> Create([FromBody] GameRequest request, CancellationToken ct)
    {
        var game = await gameService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = game.Id }, game);
    }

    /// <summary>Updates a game. Restricted to administrators.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> Update(Guid id, [FromBody] GameRequest request, CancellationToken ct)
    {
        var ok = await gameService.UpdateAsync(id, request, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Deletes a game. Restricted to administrators.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await gameService.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
