using Microsoft.AspNetCore.Mvc;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Excepciones;
using Veterinaria.Negocio.Servicios;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropietariosController : ControllerBase
{
    private readonly IPropietarioServicio _servicio;
    private readonly IMascotaServicio _mascotas;

    public PropietariosController(IPropietarioServicio servicio, IMascotaServicio mascotas)
    {
        _servicio = servicio;
        _mascotas = mascotas;
    }

    [HttpGet]
    public async Task<ActionResult<List<PropietarioDto>>> Get() =>
        Ok(await _servicio.ObtenerTodosAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PropietarioDto>> GetPorId(int id)
    {
        var dto = await _servicio.ObtenerPorIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    // Ruta anidada: api/propietarios/{id}/mascotas
    [HttpGet("{id:int}/mascotas")]
    public async Task<ActionResult<List<MascotaDto>>> GetMascotas(int id)
    {
        var lista = await _mascotas.ObtenerPorPropietarioAsync(id);
        return lista is null ? NotFound() : Ok(lista);
    }

    [HttpPost]
    public async Task<ActionResult<PropietarioDto>> Post(PropietarioCrearDto dto)
    {
        try
        {
            var creado = await _servicio.CrearAsync(dto);
            return CreatedAtAction(nameof(GetPorId), new { id = creado.Id }, creado);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, PropietarioActualizarDto dto) =>
        await _servicio.ActualizarAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            return await _servicio.EliminarAsync(id) ? NoContent() : NotFound();
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }
}
