using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.Dtos;

public class MascotaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Especie { get; set; } = null!;
    public string? Raza { get; set; }
    public int Edad { get; set; }
    public decimal Peso { get; set; }
    public DateOnly FechaRegistro { get; set; }
    public int PropietarioId { get; set; }
    public string Propietario { get; set; } = null!;
}

public class MascotaCrearDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80, MinimumLength = 2)]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "La especie es obligatoria")]
    [StringLength(40)]
    public string Especie { get; set; } = null!;

    [StringLength(60)]
    public string? Raza { get; set; }

    [Range(0, 40, ErrorMessage = "La edad debe estar entre 0 y 40")]
    public int Edad { get; set; }

    [Range(0.01, 200, ErrorMessage = "El peso debe estar entre 0.01 y 200 kg")]
    public decimal Peso { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Propietario inválido")]
    public int PropietarioId { get; set; }
}

public class MascotaActualizarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80, MinimumLength = 2)]
    public string Nombre { get; set; } = null!;

    [StringLength(60)]
    public string? Raza { get; set; }

    [Range(0, 40, ErrorMessage = "La edad debe estar entre 0 y 40")]
    public int Edad { get; set; }

    [Range(0.01, 200, ErrorMessage = "El peso debe estar entre 0.01 y 200 kg")]
    public decimal Peso { get; set; }
}
