using CatalogAPI.Data;
using CatalogAPI.Domain;
using CatalogAPI.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Services;

public interface IGameService
{
    Task<GameResponse> CreateAsync(GameRequest request, CancellationToken ct);
    Task<IReadOnlyList<GameResponse>> GetAllAsync(CancellationToken ct);
    Task<GameResponse?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> UpdateAsync(Guid id, GameRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

public class GameService(CatalogDbContext db) : IGameService
{
    public async Task<GameResponse> CreateAsync(GameRequest request, CancellationToken ct)
    {
        var game = new Game
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Price = request.Price,
            Genre = request.Genre
        };
        db.Games.Add(game);
        await db.SaveChangesAsync(ct);
        return Map(game);
    }

    public async Task<IReadOnlyList<GameResponse>> GetAllAsync(CancellationToken ct) =>
        await db.Games.AsNoTracking().OrderBy(g => g.Title).Select(g => Map(g)).ToListAsync(ct);

    public async Task<GameResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var game = await db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, ct);
        return game is null ? null : Map(game);
    }

    public async Task<bool> UpdateAsync(Guid id, GameRequest request, CancellationToken ct)
    {
        var game = await db.Games.FirstOrDefaultAsync(g => g.Id == id, ct);
        if (game is null) return false;

        game.Title = request.Title.Trim();
        game.Description = request.Description;
        game.Price = request.Price;
        game.Genre = request.Genre;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var game = await db.Games.FirstOrDefaultAsync(g => g.Id == id, ct);
        if (game is null) return false;
        db.Games.Remove(game);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static GameResponse Map(Game g) => new()
    {
        Id = g.Id,
        Title = g.Title,
        Description = g.Description,
        Price = g.Price,
        Genre = g.Genre
    };
}
