using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Api.Configurations.Extensions;

// Document transformer do OpenAPI NATIVO (.NET 10 / Microsoft.OpenApi 2.x).
// Registra o esquema de segurança Bearer (JWT) e o exige em todas as operações.
// Esta via serializa o $ref corretamente (ao contrário do Swashbuckle 10 + OpenApi 2.x),
// então o botão "Authorize" do Scalar/Swagger passa a enviar o header Authorization.
public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Cole apenas o token JWT (sem a palavra 'Bearer')."
        };

        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        };

        foreach (var pathItem in document.Paths.Values)
        {
            if (pathItem.Operations is null) continue;

            foreach (var operation in pathItem.Operations.Values)
            {
                operation.Security ??= [];
                operation.Security.Add(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
