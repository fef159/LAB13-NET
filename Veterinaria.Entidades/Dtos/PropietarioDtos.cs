using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.Dtos;

public class PropietarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Documento { get; set; } = null!;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public int TotalMascotas { get; set; }
}

public class PropietarioCrearDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 3)]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El documento es obligatorio")]
    [StringLength(20, MinimumLength = 5)]
    public string Documento { get; set; } = null!;

    [StringLength(20)]
    public string? Telefono { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
}

public class PropietarioActualizarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 3)]
    public string Nombre { get; set; } = null!;

    [StringLength(20)]
    public string? Telefono { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
}
