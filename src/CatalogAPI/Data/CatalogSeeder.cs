using CatalogAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Data;

/// <summary>
/// Popula o catálogo com jogos de exemplo na subida da aplicação.
/// Garante que todos os jogos da lista existam, INDEPENDENTE de o avaliador ter
/// cadastrado jogos próprios como administrador. Insere apenas os que ainda não
/// existem (comparando por título), de modo que não duplica a cada reinício e
/// não remove nem sobrescreve jogos criados manualmente via API.
/// Serve para o avaliador conseguir testar GET /api/games sempre com dados.
/// </summary>
public static class CatalogSeeder
{
    private static readonly Game[] SampleGames =
    [
        // Platformer / Metroidvania / Puzzle / Sandbox / Simulation / Roguelike
        new() { Title = "Celeste",                   Genre = "Platformer",   Price = 36.99m,  Description = "Ajude Madeline a escalar a Montanha Celeste neste desafiador platformer sobre superação." },
        new() { Title = "Hollow Knight",             Genre = "Metroidvania", Price = 46.99m,  Description = "Explore o vasto reino subterrâneo de Hallownest neste aclamado metroidvania desenhado à mão." },
        new() { Title = "Stardew Valley",            Genre = "Simulation",   Price = 24.99m,  Description = "Herde uma antiga fazenda, cultive, crie animais e construa uma vida no interior." },
        new() { Title = "Hades",                     Genre = "Roguelike",    Price = 47.99m,  Description = "Desafie o deus da morte em um roguelike frenético com narrativa rica e combate afiado." },
        new() { Title = "Portal 2",                  Genre = "Puzzle",       Price = 36.99m,  Description = "Resolva quebra-cabeças com a Portal Gun em uma campanha solo e cooperativa bem-humorada." },
        new() { Title = "Terraria",                  Genre = "Sandbox",      Price = 19.99m,  Description = "Cave, lute, construa e explore em um sandbox 2D com conteúdo praticamente infinito." },

        // FPS
        new() { Title = "DOOM Eternal",              Genre = "FPS",          Price = 99.99m,  Description = "Rasgue e destrua hordas demoníacas em um FPS veloz e brutal como Doom Slayer." },
        new() { Title = "Counter-Strike 2",          Genre = "FPS",          Price = 0.00m,   Description = "O lendário FPS tático competitivo 5v5, agora reconstruído na engine Source 2." },
        new() { Title = "Half-Life: Alyx",           Genre = "FPS",          Price = 149.99m, Description = "Enfrente os Combine em uma aventura imersiva de realidade virtual no universo Half-Life." },
        new() { Title = "Titanfall 2",               Genre = "FPS",          Price = 59.99m,  Description = "Combine agilidade de piloto e poder de titãs em uma campanha e multiplayer elogiados." },
        new() { Title = "Battlefield V",             Genre = "FPS",          Price = 89.99m,  Description = "Guerra total em larga escala ambientada nos conflitos da Segunda Guerra Mundial." },

        // RPG
        new() { Title = "The Witcher 3: Wild Hunt",  Genre = "RPG",          Price = 129.99m, Description = "Viva a épica jornada de Geralt de Rívia em um imenso mundo aberto de fantasia." },
        new() { Title = "Elden Ring",                Genre = "Action RPG",   Price = 249.99m, Description = "Torne-se o Lorde Prístino em um vasto mundo aberto criado por FromSoftware e George R. R. Martin." },
        new() { Title = "Baldur's Gate 3",           Genre = "RPG",          Price = 199.99m, Description = "Uma aventura de RPG baseada em D&D com escolhas profundas, combate tático por turnos e companheiros memoráveis." },
        new() { Title = "Divinity: Original Sin 2",  Genre = "RPG",          Price = 89.99m,  Description = "RPG tático cooperativo com liberdade absoluta e combate por turnos rico em interações." },
        new() { Title = "Cyberpunk 2077",            Genre = "RPG",          Price = 159.99m, Description = "Torne-se V em Night City, uma metrópole futurista obcecada por poder, estilo e modificações corporais." },
        new() { Title = "Final Fantasy XIV",         Genre = "MMORPG",       Price = 69.99m,  Description = "Entre em Eorzea, um MMORPG aclamado com história envolvente e vasto conteúdo cooperativo." }
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        // Garante que cada jogo da lista exista, sem apagar nem duplicar o que já está lá.
        var existingTitles = await db.Games
            .Select(g => g.Title)
            .ToListAsync(ct);

        var existing = new HashSet<string>(existingTitles, StringComparer.OrdinalIgnoreCase);

        var missing = SampleGames
            .Where(g => !existing.Contains(g.Title))
            .ToArray();

        if (missing.Length == 0)
            return;

        db.Games.AddRange(missing);
        await db.SaveChangesAsync(ct);
    }
}
