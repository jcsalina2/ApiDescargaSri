
using AutoMapper;
using ApiDescargaSriV9.Context;
using ApiDescargaSriV9.Dto;

namespace ApiDescargaSriV9.Helpers
{
    public class Automapper : Profile
    {
        public Automapper()
        {
            CreateMap<Empresa, EmpresaDTO>().ReverseMap();
            CreateMap<Empresa, ViewEmpresaDTO>().ReverseMap();
            CreateMap<Empresa, UpdateEmpresaDTO>().ReverseMap();
            CreateMap<DatosRecibidos, ViewDatosRecibidosDto>().ReverseMap();
            
        }
    }
}
