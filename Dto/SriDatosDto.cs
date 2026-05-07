using ApiDescargaSriV9.Services;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApiDescargaSriV9.Dto
{

    public class ViewDatosRecibidosDtolist
    {
        //[Required(ErrorMessage = "Campo Requerido")]
        //public string EmpresaApikey { get; set; }

        public int Id { get; set; }





        public List<ViewDatosRecibidosFacturaDto> viewDatosRecibidosFacturaDtos { get; set; }


        public List<ViewDatosRecibidosLIquidacionDto> viewDatosRecibidosLIquidacionDtos { get; set; }
        public List<ViewDatosRecibidosNotasCreditoDto> viewDatosRecibidosNotasCreditos { get; set; }

        public List<ViewDatosRecibidosNotasDebitoDto> viewDatosRecibidosNotasDebitos { get; set; }

        public List<ViewDatosRecibidoRetencionDto> viewDatosRecibidoRetencionDtos { get; set; }
    }

    public class ComprobantesJsonRutaXmlDto
    {
        public string Mensaje { get; set; }
        public string EmpresaRuc { get; set; }
        public string EmpresaRutaLink { get; set; }

        public List<ViewDatosRecibidosDtoJson> datosRecibidosDtoJsons { get; set; }

    }
    public class ViewDatosRecibidosDtoJson
    {
      

        public int Id { get; set; }

        public string Nro { get; set; }


        public string RUCRazonsocialemisor { get; set; }


        public string Tiposeriedecomprobante { get; set; }



        public string ClaveAccesoAutorizacion { get; set; }

        public string Fechahoradeautorizacion { get; set; }


        public string Fechaemision { get; set; }


        public string Tipoemision { get; set; }

        public object ConprobanteXmlJson { get; set; }

    }
    public class ViewDatosRecibidosDto
    {
        //[Required(ErrorMessage = "Campo Requerido")]
        //public string EmpresaApikey { get; set; }

        public int Id { get; set; }

        public string Nro { get; set; }


        public string RUCRazonsocialemisor { get; set; }


        public string Tiposeriedecomprobante { get; set; }



        public string ClaveAccesoAutorizacion { get; set; }

        public string Fechahoradeautorizacion { get; set; }


        public string Fechaemision { get; set; }


        public string Tipoemision { get; set; }

        public int Ambiente { get; set; }
        public int TipoEmision { get; set; }
        public string RazonSocial { get; set; }
        public string NombreComercial { get; set; }
        public string Ruc { get; set; }
        public string ClaveAcceso { get; set; }
        public string CodDoc { get; set; }
        public string Estab { get; set; }
        public string PtoEmi { get; set; }
        public string Secuencial { get; set; }
        public string DirMatriz { get; set; }


        public string FechaEmision { get; set; }



        public string ContribuyenteEspecial { get; set; }

        public string ObligadoContabilidad { get; set; }





        public string RazonSocialComprador { get; set; }
        public string IdentificacionComprador { get; set; }


        public string TotalSinImpuestos { get; set; }


        public string TotalDescuento { get; set; }


        public TotalConImpuestosDTO TotalConImpuestos { get; set; }

        public string Propina { get; set; }


        public string ImporteTotal { get; set; }



        public PagosDTO Pagos { get; set; }

    }
    public class ViewDatosRecibidosFacturaDto : ViewDatosRecibidosDto
    {
        //[Required(ErrorMessage = "Campo Requerido")]
        //public string EmpresaApikey { get; set; }
        public int Id { get; set; }
        public string ClaveAcceso { get; set; }

        public InformacionFacturaDto facturaxml { get; set; }



    }


    public class ViewDatosRecibidosLIquidacionDto : ViewDatosRecibidosDto
    {
        //[Required(ErrorMessage = "Campo Requerido")]
        //public string EmpresaApikey { get; set; }
        public int Id { get; set; }
        public string ClaveAcceso { get; set; }
        public LiquidacionCompra LiquidacionCompra { get; set; }



    }
    public class ViewDatosRecibidosNotasCreditoDto : ViewDatosRecibidosDto
    {
        //[Required(ErrorMessage = "Campo Requerido")]
        //public string EmpresaApikey { get; set; }
        public int Id { get; set; }
        public string ClaveAcceso { get; set; }
        public NotaCredito NotaCredito { get; set; }



    }
    public class ViewDatosRecibidosNotasDebitoDto : ViewDatosRecibidosDto
    {
        //[Required(ErrorMessage = "Campo Requerido")]
        //public string EmpresaApikey { get; set; }
        public int Id { get; set; }
        public string ClaveAcceso { get; set; }
        public NotaDebito NotaDebito { get; set; }



    }
    public class ViewDatosRecibidoRetencionDto : ViewDatosRecibidosDto
    {
        //[Required(ErrorMessage = "Campo Requerido")]
        //public string EmpresaApikey { get; set; }
        public int Id { get; set; }
        public string ClaveAcceso { get; set; }
        public ComprobanteRetencion Retencion { get; set; }



    }
    public class MD5Clas
    {

        public string Createsha56(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.SHA256 sHA256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = sHA256.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes); // .NET 5 +

                // Convert the byte array to hexadecimal string prior to .NET 5
                // StringBuilder sb = new System.Text.StringBuilder();
                // for (int i = 0; i < hashBytes.Length; i++)
                // {
                //     sb.Append(hashBytes[i].ToString("X2"));
                // }
                // return sb.ToString();
            }
        }


    }

    public class GetEmpresaDTO
    {
        public string EmpresaRuc { get; set; }


    }
    public class RucEmpresaPrincipal
    {
        public static string RucEmpresa = "";



    }
    public class RootViewDatos
    {
        public ViewDatosRecibidosDto[] viewDatosRecibidos { get; set; }
    }


    public class EmpresaDTO
    {

        public string EmpresaRuc { get; set; }


        public string EmpresaNombre { get; set; }



        public bool? GetSRIElectrnicosRecibido { get; set; }
        public bool? GetSRIElectrnicosEmitidos { get; set; }

    }
    public class ViewEmpresaDTO
    {
        public string EmpresaRuc { get; set; }
        public string EmpresaNombre { get; set; }
        public DateTime EmpresaFechaRegistro { get; set; }
        public DateTime EmpresaFechaFin { get; set; }
        public bool? EmpresaEstado { get; set; }

        public string EmpresaApikey { get; set; }
        public bool? GetSRIElectrnicosRecibido { get; set; }

        public bool? GetSRIElectrnicosEmitidos { get; set; }

    }

    public class UpdateEmpresaDTO : EmpresaDTO
    {


    }


    public class SriDatosRecibidosDto
    {

        public string Usuario { get; set; }

        /// <summary>Opcional. Solo necesaria si el flujo descarga desde el SRI (Selenium).</summary>
        public string? Password { get; set; }

        public int Dia { get; set; }



        public string Anio { get; set; }


        public int Mes { get; set; }

        public int Comprobante { get; set; }

        //public string apikey { get; set; }

    }
   
    public class SriDatosRecibidosTablaDto : SriDatosRecibidosDto
    {
        public string apikey { get; set; }


    }

    public class SriDatosEmitidosDto
    {
        //[Required(ErrorMessage = "Campo Requerido")]
        //public string EmpresaApikey { get; set; }
        public string Usuario { get; set; }
        //public string UsuarioAdicional { get; set; }
        public string Password { get; set; }

        public string Estadoautorizacion { get; set; }
        public int Comprobante { get; set; }
        public string Establecimiento { get; set; }
        public string Anio { get; set; }

        public int Mes { get; set; }



    }
    public class InfoFacturaDto
    {
        public string FechaEmision { get; set; }

        public string DirEstablecimiento { get; set; }

        public string ContribuyenteEspecial { get; set; }

        public string ObligadoContabilidad { get; set; }


        public string TipoIdentificacionComprador { get; set; }


        public string RazonSocialComprador { get; set; }
        public string IdentificacionComprador { get; set; }


        public decimal TotalSinImpuestos { get; set; }


        public decimal TotalDescuento { get; set; }


        public TotalConImpuestosDTO TotalConImpuestos { get; set; }

        public decimal Propina { get; set; }


        public decimal ImporteTotal { get; set; }



        public PagosDTO Pagos { get; set; }


    }

    public class TotalImpuestoDTO
    {

        public string Codigo { get; set; }

        public string CodigoPorcentaje { get; set; }


        public string BaseImponible { get; set; }
        public string Tarifa { get; set; }
        public string Valor { get; set; }

    }

    public class TotalConImpuestosDTO
    {
        public List<TotalImpuestoDTO> TotalImpuesto { get; set; }
    }

    public class PagosDTO
    {

        public List<PagoDto> Pago { get; set; }
    }

    public class PagoDto
    {

        public string FormaPago { get; set; }

        public string Total { get; set; }



    }


}

