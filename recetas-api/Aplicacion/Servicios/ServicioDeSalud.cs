using Recetas.Api.Aplicacion.Puertos;

namespace Recetas.Api.Aplicacion.Servicios;

public class ServicioDeSalud(IVerificadorDeSalud elVerificador)
{
    public Task<InformeDeSalud> Verifique(CancellationToken cancelacion) => elVerificador.Verifique(cancelacion);
}
