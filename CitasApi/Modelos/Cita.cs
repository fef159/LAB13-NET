namespace CitasApi.Modelos;

public record Cita(
    int Id,
    string Mascota,
    string Propietario,
    DateTime Fecha,
    string Motivo,
    string Estado);
