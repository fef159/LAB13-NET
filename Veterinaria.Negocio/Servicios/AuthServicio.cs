using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Entidades.Modelos;
using Veterinaria.Negocio.Excepciones;

namespace Veterinaria.Negocio.Servicios;

public class AuthServicio : IAuthServicio
{
    // Roles propios del negocio
    public const string Recepcionista = "Recepcionista";
    public const string Veterinario = "Veterinario";

    private readonly IUsuarioRepositorio _repo;
    private readonly IConfiguration _config;

    public AuthServicio(IUsuarioRepositorio repo, IConfiguration config)
    {
        _repo = repo;
        _config = config;
    }

    public async Task RegistrarAsync(RegistroDto dto)
    {
        // Regla de negocio: el correo es único
        if (await _repo.BuscarPorCorreoAsync(dto.Correo) is not null)
            throw new ReglaNegocioException($"Ya existe un usuario con el correo {dto.Correo}.");

        var usuario = new Usuario
        {
            Correo = dto.Correo,
            ClaveHash = BCrypt.Net.BCrypt.HashPassword(dto.Clave),
            Rol = Recepcionista // el rol lo decide el servidor, nunca el cliente
        };
        await _repo.AgregarAsync(usuario);
        await _repo.GuardarAsync();
    }

    public async Task<TokenDto?> LoginAsync(LoginDto dto)
    {
        var usuario = await _repo.BuscarPorCorreoAsync(dto.Correo);
        // Mismo resultado (null) si el correo no existe o la clave es incorrecta
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Clave, usuario.ClaveHash))
            return null;

        return GenerarToken(usuario);
    }

    private TokenDto GenerarToken(Usuario usuario)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var expira = DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpireMinutes"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
            new Claim("role", usuario.Rol)
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expira,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new TokenDto { Token = new JwtSecurityTokenHandler().WriteToken(token), Expira = expira };
    }
}
