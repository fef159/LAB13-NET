using AutoMapper;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Entidades.Modelos;

namespace Veterinaria.Negocio;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Entidad -> DTO de lectura
        CreateMap<Propietario, PropietarioDto>()
            .ForMember(d => d.TotalMascotas, o => o.MapFrom(s => s.Mascotas.Count));
        CreateMap<Mascota, MascotaDto>()
            .ForMember(d => d.Propietario, o => o.MapFrom(s => s.Propietario.Nombre));

        // DTO de entrada -> Entidad (crear)
        CreateMap<PropietarioCrearDto, Propietario>();
        CreateMap<MascotaCrearDto, Mascota>();

        // DTO de entrada -> Entidad existente (actualizar)
        CreateMap<PropietarioActualizarDto, Propietario>();
        CreateMap<MascotaActualizarDto, Mascota>();
    }
}
