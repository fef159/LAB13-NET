using Veterinaria.Entidades.Modelos;

namespace Veterinaria.Datos.Repositorios;

public interface IPropietarioRepositorio
{
    Task<List<Propietario>> ObtenerTodosAsync();
    Task<Propietario?> ObtenerPorIdAsync(int id);
    Task<bool> ExisteDocumentoAsync(string documento);
    Task<int> ContarMascotasAsync(int propietarioId);
    Task AgregarAsync(Propietario propietario);
    Task ActualizarAsync(Propietario propietario);
    Task EliminarAsync(Propietario propietario);
}
