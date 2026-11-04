using Microsoft.OpenApi;

namespace Recetas.Api.Presentacion.Configuracion;

public static class DocumentacionOpenApi
{
    public static IServiceCollection AgregueLaDocumentacionOpenApi(this IServiceCollection losServicios)
    {
        losServicios.AddOpenApi(opciones => opciones.AddDocumentTransformer((elDocumento, _, _) =>
        {
            elDocumento.Info.Title = "Recetas API";
            elDocumento.Info.Version = "Avance 2";
            elDocumento.Components ??= new();
            elDocumento.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Token devuelto por POST /api/auth/login"
                }
            };
            elDocumento.Security = [new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", elDocumento)] = []
            }];
            return Task.CompletedTask;
        }));
        return losServicios;
    }
}
