using Recetas.Api.Aplicacion.Puertos;

namespace Recetas.Api.Infraestructura.Seguridad;

public class HasheadorBCrypt : IHasheadorDeContrasenas
{
    private const int COSTO = 10;

    public string HashDeRelleno { get; } = BCrypt.Net.BCrypt.HashPassword("relleno-sin-uso-1", COSTO);

    public string Hashee(string laClave) => BCrypt.Net.BCrypt.HashPassword(laClave, COSTO);

    public bool Verifique(string laClave, string elHash) => BCrypt.Net.BCrypt.Verify(laClave, elHash);
}
