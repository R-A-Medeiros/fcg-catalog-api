using System.Text;
using Api.Configurations.Extensions;
using CatalogAPI.Consumers;
using CatalogAPI.Data;
using CatalogAPI.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");

// --- Database ---
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- Application services ---
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();

builder.Services.AddSwagger()
            .AddCorsPolicy()
            //.AddHealthCheck(builder.Environment)
            //.AddVersioning()
            .AddOptions(builder.Configuration);



// --- Auth (validates the JWT issued by UsersAPI, using the shared symmetric key) ---
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            // Casa com a claim de papel emitida pelo UsersAPI (ClaimTypes.Role),
            // para [Authorize(Roles = "administrator")] funcionar.
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.Name
        };
    });

builder.Services.AddAuthorization();

// --- Messaging (MassTransit + RabbitMQ) ---
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentProcessedConsumer>();

    // Prefixo por serviço garante filas PRÓPRIAS. Sem isto, catalog e notifications
    // compartilhariam a mesma fila "PaymentProcessed" e cada evento iria para apenas
    // um deles (competing consumers), quebrando o fan-out do PaymentProcessedEvent.
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("catalog", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

// --- Web ---
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

app.UseApiCorsPolicy()
   .UseApiHealthChecks();

app.MapOpenApi();                 // /openapi/v1.json  (documento nativo)
app.UseApiSwagger();              // Swagger UI em /swagger sobre o doc nativo
app.MapScalarApiReference(o => o.WithOpenApiRoutePattern("/openapi/v1.json")); // /scalar/v1

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "CatalogAPI" }));

app.Run();
