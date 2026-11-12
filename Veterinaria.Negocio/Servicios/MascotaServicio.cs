using AutoMapper;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Entidades.Modelos;
using Veterinaria.Negocio.Excepciones;

namespace Veterinaria.Negocio.Servicios;

public class MascotaServicio : IMascotaServicio
{
    private const int MaximoMascotasPorPropietario = 5;

    private readonly IMascotaRepositorio _repo;
    private readonly IPropietarioRepositorio _propietarios;
    private readonly IMapper _mapper;

    public MascotaServicio(IMascotaRepositorio repo, IPropietarioRepositorio propietarios, IMapper mapper)
    {
        _repo = repo;
        _propietarios = propietarios;
        _mapper = mapper;
    }

    public async Task<List<MascotaDto>> ObtenerTodasAsync(string? especie, int? edadMinima, int? edadMaxima) =>
        _mapper.Map<List<MascotaDto>>(await _repo.ObtenerTodasAsync(especie, edadMinima, edadMaxima));

    public async Task<List<MascotaDto>?> ObtenerPorPropietarioAsync(int propietarioId)
    {
        if (await _propietarios.ObtenerPorIdAsync(propietarioId) is null) return null;
        return _mapper.Map<List<MascotaDto>>(await _repo.ObtenerPorPropietarioAsync(propietarioId));
    }

    public async Task<MascotaDto?> ObtenerPorIdAsync(int id)
    {
        var m = await _repo.ObtenerPorIdAsync(id);
        return m is null ? null : _mapper.Map<MascotaDto>(m);
    }

    public async Task<MascotaDto> CrearAsync(MascotaCrearDto dto)
    {
        if (await _propietarios.ObtenerPorIdAsync(dto.PropietarioId) is null)
            throw new ReglaNegocioException($"El propietario {dto.PropietarioId} no existe.");

        // Regla de negocio 3: máximo de mascotas por propietario
        if (await _propietarios.ContarMascotasAsync(dto.PropietarioId) >= MaximoMascotasPorPropietario)
            throw new ReglaNegocioException($"Un propietario no puede tener más de {MaximoMascotasPorPropietario} mascotas.");

        // Regla de negocio 4: nombre de mascota único por propietario
        if (await _repo.ExisteNombreParaPropietarioAsync(dto.PropietarioId, dto.Nombre))
            throw new ReglaNegocioException($"El propietario ya tiene una mascota llamada {dto.Nombre}.");

        var entidad = _mapper.Map<Mascota>(dto);
        await _repo.AgregarAsync(entidad);
        return _mapper.Map<MascotaDto>(entidad);
    }

    public async Task<bool> ActualizarAsync(int id, MascotaActualizarDto dto)
    {
        var entidad = await _repo.ObtenerPorIdAsync(id);
        if (entidad is null) return false;

        if (await _repo.ExisteNombreParaPropietarioAsync(entidad.PropietarioId, dto.Nombre, id))
            throw new ReglaNegocioException($"El propietario ya tiene una mascota llamada {dto.Nombre}.");

        _mapper.Map(dto, entidad);
        await _repo.ActualizarAsync(entidad);
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var entidad = await _repo.ObtenerPorIdAsync(id);
        if (entidad is null) return false;
        await _repo.EliminarAsync(entidad);
        return true;
    }
}
