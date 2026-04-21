using ApiDescargaSriV9.CDescarga;
using ApiDescargaSriV9.Context;
using ApiDescargaSriV9.Dto;
using ApiDescargaSriV9.Helpers;
using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OfficeOpenXml;
using OfficeOpenXml.Core.ExcelPackage;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ApiDescargaSriV9.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class DescargaOdataController : ODataController
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<DescargaXmlController> _logger;
        private readonly CDescarga.CDescarga cDescarga;
        private readonly IMapper mapper;
        private readonly AplicationDbContext context;


        public DescargaOdataController(ILogger<DescargaXmlController> logger,
            CDescarga.CDescarga cDescarga, IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor, IMapper mapper,
            AplicationDbContext context)
        {
            _logger = logger;
            this.cDescarga = cDescarga;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
            this.context = context;
        }
        [HttpGet("GetTablasElectrnicosRecibidos")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableQuery]
        public async Task<ActionResult<List<ViewDatosRecibidosDto>>> GetTablasElectrnicosRecibidos([FromQuery] SriDatosRecibidosTablaDto sriDatosRecibidos)
        {
            string comprobante;

            try
            {
                switch (sriDatosRecibidos.Comprobante)
                {
                    case 0:
                        comprobante = "Todos";
                        break;
                    case 1:
                        comprobante = "Factura";
                        break;
                    case 2:
                        comprobante = "Liquidación";
                        break;
                    case 3:
                        comprobante = "NotasCrédito";
                        break;
                    case 4:
                        comprobante = "NotasDébito";
                        break;
                    case 6:
                        comprobante = "Retención";
                        break;
                    default:
                        comprobante = "Error";
                        break;
                }

                if (string.IsNullOrEmpty(sriDatosRecibidos.UsuarioAdicional))
                {
                    sriDatosRecibidos.UsuarioAdicional = "";
                }
                var empresa = await context.Empresas.FirstOrDefaultAsync(x => x.EmpresaApikey == sriDatosRecibidos.apikey);
                if (empresa == null)
                {
                    return BadRequest("No existe empresa");

                }

                var Empresapost = await context.EmpresaConsultas.FirstOrDefaultAsync(x => x.EmpresaRuc == sriDatosRecibidos.Usuario);
                if (Empresapost == null)
                {


                    EmpresaConsulta empresaConsulta = new EmpresaConsulta();
                    empresaConsulta.EmpresaRuc = sriDatosRecibidos.Usuario;
                    empresaConsulta.FkEmpresa = empresa.Id;
                    await context.AddAsync(empresaConsulta);
                    await context.SaveChangesAsync();

                    var TablasElectrnicosRecibido = cDescarga.CComprobantesElectrnicosRecibidosTablas(sriDatosRecibidos);
                    if (TablasElectrnicosRecibido != null)
                    {
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


                                    ViewDatosRecibidosDto viewDatosRecibidos = new ViewDatosRecibidosDto
                                    {

                                        Nro = values[i],
                                        RUCRazonsocialemisor = values[i + 1],
                                        Tiposeriedecomprobante = values[i + 2],
                                        ClaveAccesoAutorizacion = values[i + 3],
                                        Fechahoradeautorizacion = values[i + 4],
                                        Fechaemision = values[i + 5],
                                        Tipoemision = values[i + 6],

                                    };
                                    //var tbdatosR = mapper.Map<DatosRecibidos>(viewDatosRecibidos);
                                    //tbdatosR.FkEmpresaCon = empresaConsulta.Id;
                                    //tbdatosR.Tipoconsulta = sriDatosRecibidos.Anio + sriDatosRecibidos.Mes.ToString();
                                    //await context.AddAsync(tbdatosR);
                                    //await context.SaveChangesAsync();


                                    viewDatosRecibidosDtos.Add(datosRecibidosDto);
                                }

                            }

                            return Ok(viewDatosRecibidosDtos);

                        }
                        return BadRequest("No existe empresa");

                    }


                    return BadRequest("NO se genero Vuelva a Intentar");

                }

            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

            //if (Empresapost != null)
            //{
            //    var viewdatos = await context.DatosRecibidos.Where(x => x.FkEmpresaCon == Empresapost.Id
            //&& x.Tipoconsulta.Equals(sriDatosRecibidos.Anio + sriDatosRecibidos.Mes.ToString())).ToListAsync();
            //    if (viewdatos.Count > 0)
            //    {
            //        var viewdatosR = mapper.Map<List<ViewDatosRecibidosDto>>(viewdatos);
            //        return Ok(viewdatosR);


            //    }
            //    else
            //    {
            //        if (sriDatosRecibidos != null)
            //        {


            //            var TablasElectrnicosRecibido = cDescarga.CComprobantesElectrnicosRecibidosTablas(sriDatosRecibidos);
            //            if (TablasElectrnicosRecibido != null)
            //            {
            //                if (TablasElectrnicosRecibido.Count > 0)
            //                {
            //                    List<ViewDatosRecibidosDto> viewDatosRecibidosDtos = new List<ViewDatosRecibidosDto>();
            //                    string consultag = "";

            //                    foreach (var item in TablasElectrnicosRecibido)
            //                    {
            //                        consultag = item;

            //                        consultag = consultag.Replace("[", "");
            //                        consultag = consultag.Replace("]", "");
            //                        consultag = consultag.Replace("\"", "");

            //                        consultag = consultag.RemoveWhiteSpaces();
            //                        List<string> values = consultag.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            //                        for (int i = 0; i < values.Count; i += 7)
            //                        {


            //                            ViewDatosRecibidosDto datosRecibidosDto = new ViewDatosRecibidosDto
            //                            {
            //                                Id = Convert.ToInt32(values[i]),
            //                                Nro = values[i],
            //                                RUCRazonsocialemisor = values[i + 1],
            //                                Tiposeriedecomprobante = values[i + 2],
            //                                ClaveAccesoAutorizacion = values[i + 3],
            //                                Fechahoradeautorizacion = values[i + 4],
            //                                Fechaemision = values[i + 5],
            //                                Tipoemision = values[i + 6],

            //                            };


            //                            ViewDatosRecibidosDto viewDatosRecibidos = new ViewDatosRecibidosDto
            //                            {

            //                                Nro = values[i],
            //                                RUCRazonsocialemisor = values[i + 1],
            //                                Tiposeriedecomprobante = values[i + 2],
            //                                ClaveAccesoAutorizacion = values[i + 3],
            //                                Fechahoradeautorizacion = values[i + 4],
            //                                Fechaemision = values[i + 5],
            //                                Tipoemision = values[i + 6],

            //                            };
            //                            var tbdatosR = mapper.Map<DatosRecibidos>(viewDatosRecibidos);
            //                            tbdatosR.FkEmpresaCon = Empresapost.Id;
            //                            tbdatosR.Tipoconsulta = sriDatosRecibidos.Anio + sriDatosRecibidos.Mes.ToString();
            //                            await context.AddAsync(tbdatosR);
            //                            await context.SaveChangesAsync();


            //                            viewDatosRecibidosDtos.Add(datosRecibidosDto);
            //                        }

            //                    }

            //                    return Ok(viewDatosRecibidosDtos);

            //                }

            //            }

            //            return BadRequest("NO se genero Vuelva a Intentar");

            //        }


            //    }


            //}
            return BadRequest("NO se genero Vuelva a Intentar");

        }

        [HttpGet("GetTablasElectrnicosRecibidosXml")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableQuery]
        public async Task<ActionResult<List<ViewDatosRecibidosDto>>> GetTablasElectrnicosRecibidosXml([FromQuery] SriDatosRecibidosDto sriDatosRecibidos)
        {
            string comprobante;



            switch (sriDatosRecibidos.Comprobante)
            {
                case 0:
                    comprobante = "Todos";
                    break;
                case 1:
                    comprobante = "Factura";
                    break;
                case 2:
                    comprobante = "Liquidación";
                    break;
                case 3:
                    comprobante = "NotasCrédito";
                    break;
                case 4:
                    comprobante = "NotasDébito";
                    break;
                case 6:
                    comprobante = "Retención";
                    break;
                default:
                    comprobante = "Error";
                    break;
            }

            if (string.IsNullOrEmpty(sriDatosRecibidos.UsuarioAdicional))
            {
                sriDatosRecibidos.UsuarioAdicional = "";
            }

            var TablasElectrnicosRecibido = cDescarga.CComprobantesElectrnicosRecibidosTablasXml(sriDatosRecibidos, comprobante);
            if (TablasElectrnicosRecibido != null)
            {
                string directorioArchivoPrincipal = Path.Combine(webHostEnvironment.WebRootPath, sriDatosRecibidos.Usuario);
                string directorioArchivo = Path.Combine(directorioArchivoPrincipal, "CarpetaXmlRecibidos" + sriDatosRecibidos.Anio.ToString() + sriDatosRecibidos.Mes.ToString() + comprobante);


                if (!Directory.Exists(directorioArchivoPrincipal))
                {
                    Directory.CreateDirectory(directorioArchivoPrincipal);
                }
                if (!Directory.Exists(directorioArchivo))
                {
                    Directory.CreateDirectory(directorioArchivo);
                }
                if (TablasElectrnicosRecibido.Count > 0)
                {
                    string json = JsonConvert.SerializeObject(TablasElectrnicosRecibido, Formatting.Indented);

                    try
                    {


                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ViewDatosRecibidosDto>>(json);
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Comprobantes");

                        // Escribir las cabeceras en la primera fila
                        var headers = new List<string>()
    {
        "Id",
        "Nro",
        "RUC/Razón Social Emisor",
        "Tipo de Serie de Comprobante",
        "Clave de Acceso/Autorización",
        "Fecha y Hora de Autorización",
        "Fecha de Emisión",
        "Tipo de Emisión",
        "Ambiente",
        "Tipo de Emisión",
        "Razón Social",
        "Nombre Comercial",
        "RUC",
        "Clave de Acceso",
        "CodDoc",
        "Estab",
        "PtoEmi",
        "Secuencial",
        "Dir Matriz",
        "Fecha de Emisión",
        "Contribuyente Especial",
        "Obligado Contabilidad",
        "Razón Social Comprador",
        "Identificación Comprador",
        "Total Sin Impuestos",
        "Total Descuento",
        "Código de Impuesto",
        "Código de Porcentaje",
        "Base Imponible",
        "Tarifa",
        "Valor",
        "Propina",
        "Importe Total",
        "Forma de Pago",
        "Total Pago"
    };

                        for (int i = 0; i < headers.Count; i++)
                        {
                            worksheet.Cell(1, i + 1).Value = headers[i];
                        }

                        // Escribir los datos en filas
                        for (int i = 0; i < data.Count; i++)
                        {
                            var comprobante1 = data[i];

                            worksheet.Cell(i + 2, 1).Value = comprobante1.Id;
                            worksheet.Cell(i + 2, 2).Value = comprobante1.Nro;
                            worksheet.Cell(i + 2, 3).Value = comprobante1.RUCRazonsocialemisor;
                            worksheet.Cell(i + 2, 4).Value = comprobante1.Tiposeriedecomprobante;
                            worksheet.Cell(i + 2, 5).Value = comprobante1.ClaveAccesoAutorizacion;
                            worksheet.Cell(i + 2, 6).Value = comprobante1.Fechahoradeautorizacion;
                            worksheet.Cell(i + 2, 7).Value = comprobante1.Fechaemision;
                            worksheet.Cell(i + 2, 8).Value = comprobante1.Tipoemision;
                            worksheet.Cell(i + 2, 9).Value = comprobante1.Ambiente;
                            worksheet.Cell(i + 2, 10).Value = comprobante1.TipoEmision;
                            worksheet.Cell(i + 2, 11).Value = comprobante1.RazonSocial;
                            worksheet.Cell(i + 2, 12).Value = comprobante1.NombreComercial;
                            worksheet.Cell(i + 2, 13).Value = comprobante1.Ruc;
                            worksheet.Cell(i + 2, 14).Value = comprobante1.ClaveAcceso;
                            worksheet.Cell(i + 2, 15).Value = comprobante1.CodDoc;
                            worksheet.Cell(i + 2, 16).Value = comprobante1.Estab;
                            worksheet.Cell(i + 2, 17).Value = comprobante1.PtoEmi;
                            worksheet.Cell(i + 2, 18).Value = comprobante1.Secuencial;
                            worksheet.Cell(i + 2, 19).Value = comprobante1.DirMatriz;
                            worksheet.Cell(i + 2, 20).Value = comprobante1.FechaEmision;
                            worksheet.Cell(i + 2, 21).Value = comprobante1.ContribuyenteEspecial;
                            worksheet.Cell(i + 2, 22).Value = comprobante1.ObligadoContabilidad;
                            worksheet.Cell(i + 2, 23).Value = comprobante1.RazonSocialComprador;
                            worksheet.Cell(i + 2, 24).Value = comprobante1.IdentificacionComprador;
                            worksheet.Cell(i + 2, 25).Value = comprobante1.TotalSinImpuestos;
                            worksheet.Cell(i + 2, 26).Value = comprobante1.TotalDescuento;

                            // Código de Impuesto, Código de Porcentaje, Base Imponible, Tarifa, Valor (en caso de múltiples impuestos)
                            var totalImpuesto = comprobante1.TotalConImpuestos.TotalImpuesto.FirstOrDefault();
                            if (totalImpuesto != null)
                            {
                                worksheet.Cell(i + 2, 27).Value = totalImpuesto.Codigo;
                                worksheet.Cell(i + 2, 28).Value = totalImpuesto.CodigoPorcentaje;
                                worksheet.Cell(i + 2, 29).Value = totalImpuesto.BaseImponible;
                                worksheet.Cell(i + 2, 30).Value = totalImpuesto.Tarifa;
                                worksheet.Cell(i + 2, 31).Value = totalImpuesto.Valor;
                            }

                            worksheet.Cell(i + 2, 32).Value = comprobante1.Propina;
                            worksheet.Cell(i + 2, 33).Value = comprobante1.ImporteTotal;

                            // Forma de Pago, Total Pago (en caso de múltiples pagos)
                            var pago = comprobante1.Pagos.Pago.FirstOrDefault();
                            if (pago != null)
                            {
                                worksheet.Cell(i + 2, 34).Value = pago.FormaPago;
                                worksheet.Cell(i + 2, 35).Value = pago.Total;
                            }
                        }
                        workbook.SaveAs(directorioArchivo + "/comprobantes.xlsx");
                        if (!System.IO.File.Exists(directorioArchivo + "/comprobantes.xlsx"))
                        {
                            return NotFound();
                        }

                        // Lee el contenido del archivo PDF
                        var urlActual = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
                        var urlParaBD = Path.Combine(urlActual, directorioArchivo, "comprobantes.xlsx").Replace("\\", " /");
                        // Guardar el archivo Excel en un MemoryStream
                        return Ok(urlParaBD);
                    }
                    catch (Exception ex)
                    {
                        // Manejar cualquier error que pueda ocurrir durante la conversión
                        return BadRequest(ex.Message);
                    }

                    //var workbook = new XLWorkbook();
                    //var worksheet = workbook.Worksheets.Add("Sheet1");

                    //var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(numbers);

                    //int row = 1;
                    //foreach (var kvp in data)
                    //{
                    //    worksheet.Cell(row, 1).Value = kvp.Key;
                    //    worksheet.Cell(row, 2).Value = GetValueAsString(kvp.Value);
                    //    row++;
                    //}

                    //using (var stream = new MemoryStream())
                    //{
                    //    workbook.SaveAs(stream);
                    //    stream.Seek(0, SeekOrigin.Begin);
                    //    var content = stream.ToArray();

                    //    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "data.xlsx");
                    //}
                    // Create a new Excel workbook


                    // Save the Excel workbook




                }
                return BadRequest("No existe empresa");

            }


            return BadRequest("NO se genero Vuelva a Intentar");

        }
        [HttpGet("GetTablasElectrnicosRecibidosXmlJson")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableQuery]
        public async Task<ActionResult<ComprobantesJsonRutaXmlDto>> GetTablasElectrnicosRecibidosXmlJson([FromQuery] SriDatosRecibidosDto sriDatosRecibidos)
        {
            string comprobante;



            switch (sriDatosRecibidos.Comprobante)
            {
                case 0:
                    comprobante = "Todos";
                    break;
                case 1:
                    comprobante = "Factura";
                    break;
                case 2:
                    comprobante = "Liquidación";
                    break;
                case 3:
                    comprobante = "NotasCrédito";
                    break;
                case 4:
                    comprobante = "NotasDébito";
                    break;
                case 6:
                    comprobante = "Retención";
                    break;
                default:
                    comprobante = "Error";
                    break;
            }

            if (string.IsNullOrEmpty(sriDatosRecibidos.UsuarioAdicional))
            {
                sriDatosRecibidos.UsuarioAdicional = "";
            }

            var TablasElectrnicosRecibido = cDescarga.CComprobantesElectrnicosRecibidosTablasXmlJson(sriDatosRecibidos, comprobante);
            if (TablasElectrnicosRecibido != null)
            {
                var fileBytes = await System.IO.File.ReadAllBytesAsync(TablasElectrnicosRecibido.EmpresaRutaLink);
                var base64Zip = Convert.ToBase64String(fileBytes);
                var downloadLink = $"data:application/zip;base64,{base64Zip}";
               

                // Return the file as a byte array with the appropriate MIME type

                TablasElectrnicosRecibido.EmpresaRutaLink= downloadLink.ToString();

              
                return TablasElectrnicosRecibido;





            }
            return BadRequest("No existe Comprobantes");






        }

    }


}