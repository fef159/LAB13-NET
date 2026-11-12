using Veterinaria.Entidades.Modelos;

namespace Veterinaria.Datos.Repositorios;

public interface IMascotaRepositorio
{
    Task<List<Mascota>> ObtenerTodasAsync(string? especie, int? edadMinima, int? edadMaxima);
    Task<List<Mascota>> ObtenerPorPropietarioAsync(int propietarioId);
    Task<Mascota?> ObtenerPorIdAsync(int id);
    Task<bool> ExisteNombreParaPropietarioAsync(int propietarioId, string nombre, int? excluirId = null);
    Task AgregarAsync(Mascota mascota);
    Task ActualizarAsync(Mascota mascota);
    Task EliminarAsync(Mascota mascota);
}
