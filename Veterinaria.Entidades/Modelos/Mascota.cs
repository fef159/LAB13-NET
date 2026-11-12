using System;
using System.Collections.Generic;

namespace Veterinaria.Entidades.Modelos;

public partial class Mascota
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Especie { get; set; } = null!;

    public string? Raza { get; set; }

    public int Edad { get; set; }

    public decimal Peso { get; set; }

    public DateOnly FechaRegistro { get; set; }

    public int PropietarioId { get; set; }

    public virtual Propietario Propietario { get; set; } = null!;
}

