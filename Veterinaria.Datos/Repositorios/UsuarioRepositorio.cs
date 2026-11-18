using Microsoft.EntityFrameworkCore;
using Veterinaria.Entidades.Modelos;

namespace Veterinaria.Datos.Repositorios;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly VeterinariaContext _context;
    public UsuarioRepositorio(VeterinariaContext context) => _context = context;

    public async Task<Usuario?> BuscarPorCorreoAsync(string correo) =>
        await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);

    public async Task AgregarAsync(Usuario usuario) =>
        await _context.Usuarios.AddAsync(usuario);

    public async Task GuardarAsync() =>
        await _context.SaveChangesAsync();
}
