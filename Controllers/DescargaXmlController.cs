   using ApiDescargaSriV9.CDescarga;
using ApiDescargaSriV9.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ApiDescargaSriV9.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class DescargaXmlController : ControllerBase
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<DescargaXmlController> _logger;
        private readonly CDescarga.CDescarga cDescarga;

        public DescargaXmlController(ILogger<DescargaXmlController> logger, CDescarga.CDescarga cDescarga, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            this.cDescarga = cDescarga;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sriDatosRecibidos"></param>
        /// <returns></returns>
        [HttpGet("GetXmlFolderElectrnicosRecibidos")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult GetXmlFolderElectrnicosRecibidos([FromQuery] SriDatosRecibidosDto sriDatosRecibidos)
        {
            if (sriDatosRecibidos == null)
                return BadRequest("Solicitud no v�lida.");

            if (!TryValidateRecibidosQuery(sriDatosRecibidos, out var validationError))
                return BadRequest(validationError);

            if (!TryResolveSafeDownloadRootByRuc(sriDatosRecibidos.Usuario, out var directorioArchivoPrincipal))
                return BadRequest("RUC o ruta no permitidos.");

            if (!TryGetComprobanteRecibidos(sriDatosRecibidos.Comprobante, out var comprobante))
                return BadRequest("Comprobante no v�lido.");

            var nombreCarpeta = sriDatosRecibidos.Dia > 0
                ? "CarpetaXmlRecibidos" + sriDatosRecibidos.Anio + sriDatosRecibidos.Mes + sriDatosRecibidos.Dia + comprobante
                : "CarpetaXmlRecibidos" + sriDatosRecibidos.Anio + sriDatosRecibidos.Mes + comprobante;
            var directorioArchivo = Path.Combine(directorioArchivoPrincipal, nombreCarpeta);

            if (HasXmlFiles(directorioArchivo))
                return Ok("todo bien");

            if (!Directory.Exists(directorioArchivoPrincipal))
                Directory.CreateDirectory(directorioArchivoPrincipal);
            if (!Directory.Exists(directorioArchivo))
                Directory.CreateDirectory(directorioArchivo);

            var rutaCarpeta = cDescarga.CComprobantesElectrnicosRecibidos(sriDatosRecibidos, comprobante);
            if (!string.IsNullOrWhiteSpace(rutaCarpeta) &&
                Directory.Exists(rutaCarpeta) &&
                IsPathInsideRoot(directorioArchivoPrincipal, rutaCarpeta) &&
                HasXmlFiles(rutaCarpeta))
            {
                return Ok("todo bien");
            }

            if (HasXmlFiles(directorioArchivo))
                return Ok("todo bien");

            return BadRequest("NO se genero Vuelva a Intentar");







        }

        private static bool TryValidateRecibidosQuery(SriDatosRecibidosDto dto, out string? error)
        {
            error = null;
            if (dto.Comprobante is not (0 or 1 or 2 or 3 or 4 or 6))
            {
                error = "Comprobante no v�lido.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(dto.Anio) || dto.Anio.Length != 4 || !dto.Anio.All(char.IsDigit))
            {
                error = "A�o no v�lido.";
                return false;
            }
            if (!int.TryParse(dto.Anio, out var year) || year < 1990 || year > 2100)
            {
                error = "A�o fuera de rango.";
                return false;
            }
            if (dto.Mes is < 1 or > 12)
            {
                error = "Mes no v�lido.";
                return false;
            }
            if (dto.Dia < 0 || dto.Dia > 31)
            {
                error = "D�a no v�lido.";
                return false;
            }
            if (dto.Dia > 0 && int.TryParse(dto.Anio, out var parsedYear))
            {
                var maxDay = DateTime.DaysInMonth(parsedYear, dto.Mes);
                if (dto.Dia > maxDay)
                {
                    error = "D�a fuera de rango para el mes.";
                    return false;
                }
            }
            return true;
        }

        private static bool TryGetComprobanteRecibidos(int comprobanteId, out string comprobante)
        {
            switch (comprobanteId)
            {
                case 0:
                    comprobante = "Todos";
                    return true;
                case 1:
                    comprobante = "Factura";
                    return true;
                case 2:
                    comprobante = "Liquidaci�n";
                    return true;
                case 3:
                    comprobante = "NotasCr�dito";
                    return true;
                case 4:
                    comprobante = "NotasD�bito";
                    return true;
                case 6:
                    comprobante = "Retenci�n";
                    return true;
                default:
                    comprobante = string.Empty;
                    return false;
            }
        }

        private static bool IsPathInsideRoot(string rootPath, string candidatePath)
        {
            var fullRoot = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var fullCandidate = Path.GetFullPath(candidatePath);
            var rel = Path.GetRelativePath(fullRoot, fullCandidate);
            return !rel.Equals("..", StringComparison.Ordinal) &&
                   !rel.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
                   !Path.IsPathRooted(rel);
        }

        private bool TryResolveSafeDownloadRootByRuc(string? ruc, out string userContentRoot)
        {
            userContentRoot = "";
            if (string.IsNullOrWhiteSpace(ruc))
                return false;

            var cleanRuc = ruc.Trim();
            if (cleanRuc.Length is < 10 or > 13 || !cleanRuc.All(char.IsDigit))
                return false;

            var root = webHostEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(root))
                return false;

            var fullRoot = Path.GetFullPath(root);
            userContentRoot = Path.GetFullPath(Path.Combine(fullRoot, "Descargas", cleanRuc));
            var rel = Path.GetRelativePath(fullRoot, userContentRoot);
            if (rel.Equals("..", StringComparison.Ordinal) || rel.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                userContentRoot = "";
                return false;
            }
            return true;
        }

        [HttpGet("GetTablasElectrnicosRecibidos")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ViewDatosRecibidosDto>> GetTablasElectrnicosRecibidos([FromQuery] SriDatosRecibidosDto sriDatosRecibidos)
        {

            if (string.IsNullOrEmpty(sriDatosRecibidos.UsuarioAdicional))
            {
                sriDatosRecibidos.UsuarioAdicional = "";
            }


            if (sriDatosRecibidos != null)
            {


                var TablasElectrnicosRecibido = cDescarga.CComprobantesElectrnicosRecibidosTablas(sriDatosRecibidos);
                if (TablasElectrnicosRecibido.Count > 0)
                {
                    List<ViewDatosRecibidosDto> viewDatosRecibidosDtos = new List<ViewDatosRecibidosDto>();
                    string consultag = "";


                    foreach (var item in TablasElectrnicosRecibido)
                    {
                        consultag = item;

                        consultag = consultag.Replace("[", "");
                        consultag = consultag.Replace("]", "");
                        consultag = consultag.Replace("\"", "");

                        consultag = consultag.RemoveWhiteSpaces();
                        List<string> values = consultag.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();


                        for (int i = 0; i < values.Count; i += 7)
                        {


                            ViewDatosRecibidosDto datosRecibidosDto = new ViewDatosRecibidosDto
                            {
                                Id = Convert.ToInt32(values[i]),
                                Nro = values[i],
                                RUCRazonsocialemisor = values[i + 1],
                                Tiposeriedecomprobante = values[i + 2],
                                ClaveAccesoAutorizacion = values[i + 3],
                                Fechahoradeautorizacion = values[i + 4],
                                Fechaemision = values[i + 5],
                                Tipoemision = values[i + 6],

                            };

                            viewDatosRecibidosDtos.Add(datosRecibidosDto);
                        }



                    }

                    return Ok(viewDatosRecibidosDtos);

                }


                return BadRequest("NO se genero Vuelva a Intentar");

            }

            return BadRequest("NO se genero Vuelva a Intentar");



        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sriDatosEmitidosDto"></param>
        /// <returns></returns>
        [HttpGet("GetXmlFolderComprobantesElectrnicosEmitidos")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult GetXmlFolderComprobantesElectrnicosEmitidos([FromQuery] SriDatosEmitidosDto sriDatosEmitidosDto)
        {
            if (sriDatosEmitidosDto == null)
                return BadRequest("Solicitud no v�lida.");

            if (!TryGetComprobanteRecibidos(sriDatosEmitidosDto.Comprobante, out var comprobante))
                return BadRequest("Comprobante no v�lido.");

            if (!TryResolveSafeDownloadRootByRuc(sriDatosEmitidosDto.Usuario, out var directorioArchivoPrincipal))
                return BadRequest("RUC o ruta no permitidos.");

            string directorioArchivo = Path.Combine(directorioArchivoPrincipal, sriDatosEmitidosDto.Anio + sriDatosEmitidosDto.Mes + comprobante);
            if (HasXmlFiles(directorioArchivo))
                return Ok("todo bien");
            if (!Directory.Exists(directorioArchivoPrincipal))
            {
                Directory.CreateDirectory(directorioArchivoPrincipal);

            }



            if (!Directory.Exists(directorioArchivo))
            {
                Directory.CreateDirectory(directorioArchivo);
            }



            if (sriDatosEmitidosDto != null)
            {

                var ruta = cDescarga.CComprobantesElectrnicosEmitidos(sriDatosEmitidosDto, comprobante);
                if (!string.IsNullOrWhiteSpace(ruta) && Directory.Exists(ruta) && IsPathInsideRoot(directorioArchivoPrincipal, ruta) && HasXmlFiles(ruta))
                {
                    return Ok("todo bien");
                }
            }

            if (HasXmlFiles(directorioArchivo))
                return Ok("todo bien");

            return BadRequest("NO se genero Vuelva a Intentar");




        }

        private bool HasXmlFiles(string folderPath)
        {
            return Directory.Exists(folderPath) &&
                   Directory.EnumerateFiles(folderPath, "*.xml", SearchOption.TopDirectoryOnly).Any();
        }



    }
}