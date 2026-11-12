using Microsoft.AspNetCore.Mvc;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Excepciones;
using Veterinaria.Negocio.Servicios;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private readonly IMascotaServicio _servicio;

    public MascotasController(IMascotaServicio servicio) => _servicio = servicio;

    // GET api/mascotas?especie=Perro&edadMinima=1&edadMaxima=5
    [HttpGet]
    public async Task<ActionResult<List<MascotaDto>>> Get(
        [FromQuery] string? especie, [FromQuery] int? edadMinima, [FromQuery] int? edadMaxima) =>
        Ok(await _servicio.ObtenerTodasAsync(especie, edadMinima, edadMaxima));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MascotaDto>> GetPorId(int id)
    {
        var dto = await _servicio.ObtenerPorIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<MascotaDto>> Post(MascotaCrearDto dto)
    {
        try
        {
            var creada = await _servicio.CrearAsync(dto);
            return CreatedAtAction(nameof(GetPorId), new { id = creada.Id }, creada);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, MascotaActualizarDto dto)
    {
        try
        {
            return await _servicio.ActualizarAsync(id, dto) ? NoContent() : NotFound();
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) =>
        await _servicio.EliminarAsync(id) ? NoContent() : NotFound();
}
