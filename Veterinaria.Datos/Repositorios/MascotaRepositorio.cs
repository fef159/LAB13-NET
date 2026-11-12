using Microsoft.EntityFrameworkCore;
using Veterinaria.Entidades.Modelos;

namespace Veterinaria.Datos.Repositorios;

public class MascotaRepositorio : IMascotaRepositorio
{
    private readonly VeterinariaContext _context;
    public MascotaRepositorio(VeterinariaContext context) => _context = context;

    public async Task<List<Mascota>> ObtenerTodasAsync(string? especie, int? edadMinima, int? edadMaxima)
    {
        var query = _context.Mascotas.Include(m => m.Propietario).AsQueryable();
        if (!string.IsNullOrWhiteSpace(especie))
            query = query.Where(m => m.Especie == especie);
        if (edadMinima.HasValue)
            query = query.Where(m => m.Edad >= edadMinima.Value);
        if (edadMaxima.HasValue)
            query = query.Where(m => m.Edad <= edadMaxima.Value);
        return await query.OrderBy(m => m.Nombre).ToListAsync();
    }

    public async Task<List<Mascota>> ObtenerPorPropietarioAsync(int propietarioId) =>
        await _context.Mascotas.Include(m => m.Propietario)
            .Where(m => m.PropietarioId == propietarioId).OrderBy(m => m.Nombre).ToListAsync();

    public async Task<Mascota?> ObtenerPorIdAsync(int id) =>
        await _context.Mascotas.Include(m => m.Propietario).FirstOrDefaultAsync(m => m.Id == id);

    public async Task<bool> ExisteNombreParaPropietarioAsync(int propietarioId, string nombre, int? excluirId = null) =>
        await _context.Mascotas.AnyAsync(m => m.PropietarioId == propietarioId
            && m.Nombre == nombre && (excluirId == null || m.Id != excluirId));

    public async Task AgregarAsync(Mascota mascota)
    {
        _context.Mascotas.Add(mascota);
        await _context.SaveChangesAsync();
        await _context.Entry(mascota).Reference(m => m.Propietario).LoadAsync();
    }

    public async Task ActualizarAsync(Mascota mascota)
    {
        _context.Mascotas.Update(mascota);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Mascota mascota)
    {
        _context.Mascotas.Remove(mascota);
        await _context.SaveChangesAsync();
    }
}
