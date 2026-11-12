using System;
using System.Collections.Generic;

namespace Veterinaria.Entidades.Modelos;

public partial class Propietario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Documento { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
}

