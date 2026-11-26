using CitasApi.Datos;
using CitasApi.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CitasApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // todos los endpoints exigen un token válido
public class CitasController : ControllerBase
{
    private readonly CitasEnMemoria _citas;

    public CitasController(CitasEnMemoria citas) => _citas = citas;

    // GET api/citas
    [HttpGet]
    public ActionResult<IReadOnlyList<Cita>> Get() => Ok(_citas.ObtenerTodas());

    // GET api/citas/{id}
    [HttpGet("{id:int}")]
    public ActionResult<Cita> GetPorId(int id)
    {
        var cita = _citas.ObtenerPorId(id);
        return cita is null ? NotFound() : Ok(cita);
    }
}
