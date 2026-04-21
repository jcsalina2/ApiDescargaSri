using ApiDescargaSriV9.Services;
using System.Xml.Serialization;

namespace ApiDescargaSriV9.Dto
{
    public class InformacionFacturaDto
    {

        [XmlRoot(ElementName = "factura")]
        public class InformacionFactura
        {
            [XmlElement(ElementName = "infoTributaria")]
            public InfoTributariaFactura2Dto InfoTributaria { get; set; }
            [XmlElement(ElementName = "infoFactura")]
            public InfoFacturaFactura2DTO InfoFactura { get; set; }
        }
        public class InfoFacturaFactura2DTO
        {
            [XmlElement(ElementName = "fechaEmision")]
            public string FechaEmision { get; set; }
            [XmlElement(ElementName = "dirEstablecimiento")]
            public string DirEstablecimiento { get; set; }
            [XmlElement(ElementName = "contribuyenteEspecial")]
            public string ContribuyenteEspecial { get; set; }
            [XmlElement(ElementName = "obligadoContabilidad")]
            public string ObligadoContabilidad { get; set; }
            [XmlElement(ElementName = "comercioExterior")]
            public string ComercioExterior { get; set; }
            [XmlElement(ElementName = "incoTermFactura")]
            public string IncoTermFactura { get; set; }
            [XmlElement(ElementName = "lugarIncoTerm")]
            public string LugarIncoTerm { get; set; }
            [XmlElement(ElementName = "paisOrigen")]
            public string PaisOrigen { get; set; }
            [XmlElement(ElementName = "puertoEmbarque")]
            public string PuertoEmbarque { get; set; }
            [XmlElement(ElementName = "puertoDestino")]
            public string PuertoDestino { get; set; }
            [XmlElement(ElementName = "paisDestino")]
            public string PaisDestino { get; set; }
            [XmlElement(ElementName = "paisAdquisicion")]
            public string PaisAdquisicion { get; set; }
            [XmlElement(ElementName = "tipoIdentificacionComprador")]
            public string TipoIdentificacionComprador { get; set; }
            [XmlElement(ElementName = "guiaRemision")]
            public string GuiaRemision { get; set; }
            [XmlElement(ElementName = "razonSocialComprador")]
            public string RazonSocialComprador { get; set; }
            [XmlElement(ElementName = "identificacionComprador")]
            public string IdentificacionComprador { get; set; }
            [XmlElement(ElementName = "direccionComprador")]
            public string DireccionComprador { get; set; }
            [XmlElement(ElementName = "totalSinImpuestos")]
            public string TotalSinImpuestos { get; set; }
            [XmlElement(ElementName = "totalSubsidio")]
            public string TotalSubsidio { get; set; }
            [XmlElement(ElementName = "incoTermTotalSinImpuestos")]
            public string IncoTermTotalSinImpuestos { get; set; }
            [XmlElement(ElementName = "totalDescuento")]
            public string TotalDescuento { get; set; }
            [XmlElement(ElementName = "codDocReembolso")]
            public string CodDocReembolso { get; set; }
            [XmlElement(ElementName = "totalComprobantesReembolso")]
            public string TotalComprobantesReembolso { get; set; }
            [XmlElement(ElementName = "totalBaseImponibleReembolso")]
            public string TotalBaseImponibleReembolso { get; set; }
            [XmlElement(ElementName = "totalImpuestoReembolso")]
            public string TotalImpuestoReembolso { get; set; }
            [XmlElement(ElementName = "totalConImpuestos")]
            public TotalConImpuestosFactura2dto TotalConImpuestos { get; set; }
            [XmlElement(ElementName = "compensaciones")]
            public CompensacionesFactura2 Compensaciones { get; set; }
            [XmlElement(ElementName = "propina")]
            public string Propina { get; set; }
            [XmlElement(ElementName = "fleteInternacional")]
            public string FleteInternacional { get; set; }
            [XmlElement(ElementName = "seguroInternacional")]
            public string SeguroInternacional { get; set; }
            [XmlElement(ElementName = "gastosAduaneros")]
            public string GastosAduaneros { get; set; }
            [XmlElement(ElementName = "gastosTransporteOtros")]
            public string GastosTransporteOtros { get; set; }
            [XmlElement(ElementName = "importeTotal")]
            public string ImporteTotal { get; set; }
            [XmlElement(ElementName = "moneda")]
            public string Moneda { get; set; }
            [XmlElement(ElementName = "placa")]
            public string Placa { get; set; }
            [XmlElement(ElementName = "pagos")]
            public PagosFactura2 Pagos { get; set; }
            [XmlElement(ElementName = "valorRetIva")]
            public string ValorRetIva { get; set; }
            [XmlElement(ElementName = "valorRetRenta")]
            public string ValorRetRenta { get; set; }
        }

        [XmlRoot(ElementName = "infoTributaria")]
        public class InfoTributariaFactura2Dto
        {
            [XmlElement(ElementName = "ambiente")]
            public string Ambiente { get; set; }
            [XmlElement(ElementName = "tipoEmision")]
            public string TipoEmision { get; set; }
            [XmlElement(ElementName = "razonSocial")]
            public string RazonSocial { get; set; }
            [XmlElement(ElementName = "nombreComercial")]
            public string NombreComercial { get; set; }
            [XmlElement(ElementName = "ruc")]
            public string Ruc { get; set; }
            [XmlElement(ElementName = "claveAcceso")]
            public string ClaveAcceso { get; set; }
            [XmlElement(ElementName = "codDoc")]
            public string CodDoc { get; set; }
            [XmlElement(ElementName = "estab")]
            public string Estab { get; set; }
            [XmlElement(ElementName = "ptoEmi")]
            public string PtoEmi { get; set; }
            [XmlElement(ElementName = "secuencial")]
            public string Secuencial { get; set; }
            [XmlElement(ElementName = "dirMatriz")]
            public string DirMatriz { get; set; }
            [XmlElement(ElementName = "agenteRetencion")]
            public string AgenteRetencion { get; set; }
            [XmlElement(ElementName = "contribuyenteRimpe")]
            public string ContribuyenteRimpe { get; set; }
        }


        public class TotalImpuestoFactura2Dto
        {
            [XmlElement(ElementName = "codigo")]
            public string Codigo { get; set; }
            [XmlElement(ElementName = "codigoPorcentaje")]
            public string CodigoPorcentaje { get; set; }
            [XmlElement(ElementName = "descuentoAdicional")]
            public string DescuentoAdicional { get; set; }
            [XmlElement(ElementName = "baseImponible")]
            public string BaseImponible { get; set; }
            [XmlElement(ElementName = "tarifa")]
            public string Tarifa { get; set; }
            [XmlElement(ElementName = "valor")]
            public string Valor { get; set; }
            [XmlElement(ElementName = "valorDevolucionIva")]
            public string ValorDevolucionIva { get; set; }
        }

        [XmlRoot(ElementName = "totalConImpuestos")]
        public class TotalConImpuestosFactura2dto
        {
            [XmlElement(ElementName = "totalImpuesto")]
            public List<TotalImpuestoFactura2Dto> TotalImpuesto { get; set; }
        }
    }
}
