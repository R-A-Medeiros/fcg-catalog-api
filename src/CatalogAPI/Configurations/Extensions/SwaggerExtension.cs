using Swashbuckle.AspNetCore.SwaggerUI;

namespace Api.Configurations.Extensions;

// A documentação OpenAPI é gerada pelo pacote NATIVO do .NET 10
// (builder.Services.AddOpenApi + BearerSecuritySchemeTransformer), servida em
// /openapi/v1.json. Aqui apenas expomos duas UIs sobre esse MESMO documento:
//   - Swagger UI  -> /swagger
//   - Scalar      -> /scalar/v1  (configurado no Program via MapScalarApiReference)
// Assim o esquema Bearer (com $ref correto) aparece nas duas, e o botão Authorize
// envia o header Authorization: Bearer <token>.
public static class SwaggerExtension
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        // Sem Swashbuckle.SwaggerGen: o documento vem do OpenAPI nativo.
        return services;
    }

    public static WebApplication UseApiSwagger(this WebApplication app)
    {
        // A documentação (Swagger UI + Scalar) é exposta em todos os ambientes,
        // pois é a documentação oficial do projeto. A página de exceção detalhada
        // continua restrita a ambientes de desenvolvimento.
        if (app.Environment.EnvironmentName is "Local" or "Development")
        {
            app.UseDeveloperExceptionPage();
        }

        // Swagger UI apontando para o documento gerado pelo OpenAPI nativo.
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/openapi/v1.json", "v1");
            c.DocExpansion(DocExpansion.None);
        });

        return app;
    }
}
