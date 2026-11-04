using System.Collections.Concurrent;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Recetas.Api.Aplicacion.Puertos;

namespace Recetas.Api.Infraestructura.Seguridad;

public class OpcionesJwt
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "recetas-api";
    public string Audience { get; set; } = "recetas-spa";
    public int Horas { get; set; } = 2;

    public SymmetricSecurityKey ClaveDeFirma => new(Encoding.UTF8.GetBytes(Key));
}

public class ServicioDeTokensJwt(OpcionesJwt lasOpciones, TimeProvider elReloj) : IServicioDeTokens
{
    public TokenEmitido Cree(int elUsuarioId, string elRol)
    {
        var ahora = elReloj.GetUtcNow();
        var laExpiracion = ahora.AddHours(lasOpciones.Horas);
        var elDescriptor = new SecurityTokenDescriptor
        {
            Issuer = lasOpciones.Issuer,
            Audience = lasOpciones.Audience,
            IssuedAt = ahora.UtcDateTime,
            NotBefore = ahora.UtcDateTime,
            Expires = laExpiracion.UtcDateTime,
            Claims = new Dictionary<string, object>
            {
                ["sub"] = elUsuarioId.ToString(),
                ["rol"] = elRol,
                ["jti"] = Guid.NewGuid().ToString("N")
            },
            SigningCredentials = new SigningCredentials(lasOpciones.ClaveDeFirma, SecurityAlgorithms.HmacSha256)
        };
        return new TokenEmitido(new JsonWebTokenHandler().CreateToken(elDescriptor), laExpiracion);
    }
}

public class RevocadorDeTokensEnMemoria(TimeProvider elReloj) : IRevocadorDeTokens
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> losTokensRevocados = new();

    public void Revoque(string elIdentificador, DateTimeOffset laExpiracion)
    {
        var ahora = elReloj.GetUtcNow();
        foreach (var (elId, elVencimiento) in losTokensRevocados)
            if (elVencimiento <= ahora) losTokensRevocados.TryRemove(elId, out _);
        losTokensRevocados[elIdentificador] = laExpiracion;
    }

    public bool EstaRevocado(string? elIdentificador) => elIdentificador is not null && losTokensRevocados.ContainsKey(elIdentificador);
}
