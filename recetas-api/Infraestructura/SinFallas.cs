using Recetas.Api.Aplicacion.Puertos;

namespace Recetas.Api.Infraestructura;

public class SinFallas : IPuntoDeFalla
{
    public void PaseElPuntoDeFalla(string elPunto) { }
}
