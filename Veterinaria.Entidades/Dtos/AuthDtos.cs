using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.Dtos;

public class LoginDto
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress]
    public string Correo { get; set; } = null!;

    [Required(ErrorMessage = "La clave es obligatoria")]
    public string Clave { get; set; } = null!;
}

// Registro: no incluye el rol. El servidor lo asigna.
public class RegistroDto
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress]
    [StringLength(100)]
    public string Correo { get; set; } = null!;

    [Required(ErrorMessage = "La clave es obligatoria")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La clave debe tener al menos 8 caracteres")]
    public string Clave { get; set; } = null!;
}

public class TokenDto
{
    public string Token { get; set; } = null!;
    public DateTime Expira { get; set; }
}

public class PerfilDto
{
    public string Correo { get; set; } = null!;
    public string Rol { get; set; } = null!;
}
