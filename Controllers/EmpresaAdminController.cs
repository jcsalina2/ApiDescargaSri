
using AutoMapper;
using ApiDescargaSriV9.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ApiDescargaSriV9.Dto
{
    [ApiController]
    [Route("api/AdminEmpresa")]

    public class EmpresaAdminController : ControllerBase
    {
        DateTime currentTimePacific = TimeZoneInfo.ConvertTime(DateTime.Now,
            TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "SA Pacific Standard Time" : "America/Guayaquil"));


        private readonly AplicationDbContext context;
        private readonly IMapper mapper;

        public EmpresaAdminController(

        AplicationDbContext context,
            IMapper mapper)
        {



            this.context = context;
            this.mapper = mapper;



        }



        [HttpPost("PostEmpresaAdmin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ViewEmpresaDTO>> PostEmpresaAdmin([FromForm] EmpresaDTO ingresoEmpresaDto)
        {



            Random generator = new Random();
            string numeroGenerado = generator.Next(0, 100000000).ToString("D6");
            Empresa Empresaget = null;
            Empresaget = await context.Empresas.FirstOrDefaultAsync(x => x.EmpresaRuc.Equals(ingresoEmpresaDto.EmpresaRuc.Trim()));

            if (Empresaget == null)
            {
                try
                {

                    var EmresaPost = mapper.Map<Empresa>(ingresoEmpresaDto);
                    EmresaPost.EmpresaFechaRegistro = currentTimePacific;
                    EmresaPost.EmpresaFechaFin = currentTimePacific.AddYears(1);
                    EmresaPost.EmpresaEstado = true;
                    MD5Clas mD5 = new MD5Clas();
                    var md5 = mD5.Createsha56(numeroGenerado + EmresaPost.EmpresaRuc + EmresaPost.EmpresaNombre + DateTime.Now.ToString());
                    EmresaPost.EmpresaApikey = md5;
                    await context.AddAsync(EmresaPost);
                    await context.SaveChangesAsync();
                    return mapper.Map<Empresa, ViewEmpresaDTO>(EmresaPost);

                }
                catch (Exception ex)
                {

                    return BadRequest(ex.ToString());
                }
            }

            return BadRequest("Ya existe Empresa");

        }


        [HttpPut("PutEmpresaAdmin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ViewEmpresaDTO>> PutEmpresaAdmin([FromForm] UpdateEmpresaDTO putEmpresaDto)
        {
            Empresa Empresaget = null;
            Empresaget = await context.Empresas.FirstOrDefaultAsync(x => x.EmpresaRuc.Equals(putEmpresaDto.EmpresaRuc.Trim()));
            if (Empresaget == null)
            {
                try
                {
                    Random generator = new Random();
                    string numeroGenerado = generator.Next(0, 100000000).ToString("D6");
                    var EmresaPost = mapper.Map<Empresa>(putEmpresaDto);
                    EmresaPost.EmpresaFechaRegistro = DateTime.Now;
                    EmresaPost.EmpresaFechaFin = DateTime.Now.AddYears(1);
                    EmresaPost.EmpresaEstado = true;
                    MD5Clas mD5 = new MD5Clas();
                    var md5 = mD5.Createsha56(numeroGenerado + EmresaPost.EmpresaRuc + EmresaPost.EmpresaNombre + DateTime.Now.ToString());
                    EmresaPost.EmpresaApikey = md5;

                    await context.SaveChangesAsync();
                    return mapper.Map<Empresa, ViewEmpresaDTO>(EmresaPost);

                }
                catch (Exception ex)
                {

                    return BadRequest(ex.ToString());
                }
            }

            return BadRequest("NO existe Empresa");


        }

        [HttpGet("GetEmpresaAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ViewEmpresaDTO>> GeEmpresasCreada([FromQuery] GetEmpresaDTO getEmpresaDTO)
        {
            Empresa Empresaget = null;

            Empresaget = await context.Empresas.FirstOrDefaultAsync(x => x.EmpresaRuc.Equals(getEmpresaDTO.EmpresaRuc.Trim()));
            if (Empresaget == null)
            {
                return BadRequest("No existe Empresa ");
            }
            return mapper.Map<Empresa, ViewEmpresaDTO>(Empresaget);

        }




    }


}