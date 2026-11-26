using CitasApi.Modelos;

namespace CitasApi.Datos;

// Datos en memoria: este microservicio no usa base de datos ni clases de la API principal
public class CitasEnMemoria
{
    private readonly List<Cita> _citas =
    [
        new(1, "Max",   "Carlos Méndez",   new DateTime(2026, 11, 27,  9,  0, 0), "Vacuna anual",     "Programada"),
        new(2, "Luna",  "Carlos Méndez",   new DateTime(2026, 11, 27, 10, 30, 0), "Control de peso",  "Programada"),
        new(3, "Rocky", "Lucía Hernández", new DateTime(2026, 11, 28, 15,  0, 0), "Revisión de piel", "Confirmada"),
        new(4, "Nemo",  "Roberto Pineda",  new DateTime(2026, 11, 20, 11,  0, 0), "Consulta general", "Atendida")
    ];

    public IReadOnlyList<Cita> ObtenerTodas() => _citas;

    public Cita? ObtenerPorId(int id) => _citas.FirstOrDefault(c => c.Id == id);
}
