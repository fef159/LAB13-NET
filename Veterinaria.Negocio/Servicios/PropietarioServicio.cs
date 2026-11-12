using AutoMapper;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Entidades.Modelos;
using Veterinaria.Negocio.Excepciones;

namespace Veterinaria.Negocio.Servicios;

public class PropietarioServicio : IPropietarioServicio
{
    private readonly IPropietarioRepositorio _repo;
    private readonly IMapper _mapper;

    public PropietarioServicio(IPropietarioRepositorio repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<PropietarioDto>> ObtenerTodosAsync() =>
        _mapper.Map<List<PropietarioDto>>(await _repo.ObtenerTodosAsync());

    public async Task<PropietarioDto?> ObtenerPorIdAsync(int id)
    {
        var p = await _repo.ObtenerPorIdAsync(id);
        return p is null ? null : _mapper.Map<PropietarioDto>(p);
    }

    public async Task<PropietarioDto> CrearAsync(PropietarioCrearDto dto)
    {
        // Regla de negocio 1: el documento no puede repetirse
        if (await _repo.ExisteDocumentoAsync(dto.Documento))
            throw new ReglaNegocioException($"Ya existe un propietario con el documento {dto.Documento}.");

        var entidad = _mapper.Map<Propietario>(dto);
        await _repo.AgregarAsync(entidad);
        return _mapper.Map<PropietarioDto>(entidad);
    }

    public async Task<bool> ActualizarAsync(int id, PropietarioActualizarDto dto)
    {
        var entidad = await _repo.ObtenerPorIdAsync(id);
        if (entidad is null) return false;
        _mapper.Map(dto, entidad);
        await _repo.ActualizarAsync(entidad);
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var entidad = await _repo.ObtenerPorIdAsync(id);
        if (entidad is null) return false;

        // Regla de negocio 2: no se elimina un propietario con mascotas registradas
        if (await _repo.ContarMascotasAsync(id) > 0)
            throw new ReglaNegocioException("No se puede eliminar un propietario que tiene mascotas registradas.");

        await _repo.EliminarAsync(entidad);
        return true;
    }
}
