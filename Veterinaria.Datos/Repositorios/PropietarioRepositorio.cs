using Microsoft.EntityFrameworkCore;
using Veterinaria.Entidades.Modelos;

namespace Veterinaria.Datos.Repositorios;

public class PropietarioRepositorio : IPropietarioRepositorio
{
    private readonly VeterinariaContext _context;
    public PropietarioRepositorio(VeterinariaContext context) => _context = context;

    public async Task<List<Propietario>> ObtenerTodosAsync() =>
        await _context.Propietarios.Include(p => p.Mascotas).OrderBy(p => p.Nombre).ToListAsync();

    public async Task<Propietario?> ObtenerPorIdAsync(int id) =>
        await _context.Propietarios.Include(p => p.Mascotas).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<bool> ExisteDocumentoAsync(string documento) =>
        await _context.Propietarios.AnyAsync(p => p.Documento == documento);

    public async Task<int> ContarMascotasAsync(int propietarioId) =>
        await _context.Mascotas.CountAsync(m => m.PropietarioId == propietarioId);

    public async Task AgregarAsync(Propietario propietario)
    {
        _context.Propietarios.Add(propietario);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Propietario propietario)
    {
        _context.Propietarios.Update(propietario);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Propietario propietario)
    {
        _context.Propietarios.Remove(propietario);
        await _context.SaveChangesAsync();
    }
}
