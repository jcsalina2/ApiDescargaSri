/* 
 Licensed under the Apache License, Version 2.0

 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using ApiDescargaSriV9.Context;

namespace ApiDescargaSriV9.Services
{
    [XmlRoot(ElementName = "infoTributaria")]
    public class InfoTributariaFactura2
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

    [XmlRoot(ElementName = "totalImpuesto")]
    public class TotalImpuestoFactura2
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
    public class TotalConImpuestosFactura2
    {
        [XmlElement(ElementName = "totalImpuesto")]
        public List<TotalImpuestoFactura2> TotalImpuesto { get; set; }
    }
  
    [XmlRoot(ElementName = "compensacion")]
    public class CompensacionFactura2
    {
        [XmlElement(ElementName = "codigo")]
        public string Codigo { get; set; }
        [XmlElement(ElementName = "tarifa")]
        public string Tarifa { get; set; }
        [XmlElement(ElementName = "valor")]
        public string Valor { get; set; }
    }

    [XmlRoot(ElementName = "compensaciones")]
    public class CompensacionesFactura2
    {
        [XmlElement(ElementName = "compensacion")]
        public List<CompensacionFactura2> Compensacion { get; set; }
    }

    [XmlRoot(ElementName = "pago")]
    public class PagoFactura2
    {
        [XmlElement(ElementName = "formaPago")]
        public string FormaPago { get; set; }
        [XmlElement(ElementName = "total")]
        public string Total { get; set; }
        [XmlElement(ElementName = "plazo")]
        public string Plazo { get; set; }
        [XmlElement(ElementName = "unidadTiempo")]
        public string UnidadTiempo { get; set; }
    }

    [XmlRoot(ElementName = "pagos")]
    public class PagosFactura2
    {
        [XmlElement(ElementName = "pago")]
        public List<PagoFactura2> Pago { get; set; }
    }

    [XmlRoot(ElementName = "infoFactura")]
    public class InfoFacturaFactura2
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
        public TotalConImpuestosFactura2 TotalConImpuestos { get; set; }
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

    [XmlRoot(ElementName = "detAdicional")]
    public class DetAdicionalFactura2
    {
        [XmlAttribute(AttributeName = "nombre")]
        public string Nombre { get; set; }
        [XmlAttribute(AttributeName = "valor")]
        public string Valor { get; set; }
    }

    [XmlRoot(ElementName = "detallesAdicionales")]
    public class DetallesAdicionalesFactura2
    {
        [XmlElement(ElementName = "detAdicional")]
        public List<DetAdicionalFactura2> DetAdicional { get; set; }
    }

    [XmlRoot(ElementName = "impuesto")]
    public class ImpuestoFactura2
    {
        [XmlElement(ElementName = "codigo")]
        public string Codigo { get; set; }
        [XmlElement(ElementName = "codigoPorcentaje")]
        public string CodigoPorcentaje { get; set; }
        [XmlElement(ElementName = "tarifa")]
        public string Tarifa { get; set; }
        [XmlElement(ElementName = "baseImponible")]
        public string BaseImponible { get; set; }
        [XmlElement(ElementName = "valor")]
        public string Valor { get; set; }
    }

    [XmlRoot(ElementName = "impuestos")]
    public class ImpuestosFactura2
    {
        [XmlElement(ElementName = "impuesto")]
        public List<ImpuestoFactura2> Impuesto { get; set; }
    }

    [XmlRoot(ElementName = "detalle")]
    public class DetalleFactura2
    {
        [XmlElement(ElementName = "codigoPrincipal")]
        public string CodigoPrincipal { get; set; }
        [XmlElement(ElementName = "codigoAuxiliar")]
        public string CodigoAuxiliar { get; set; }
        [XmlElement(ElementName = "descripcion")]
        public string Descripcion { get; set; }
        [XmlElement(ElementName = "unidadMedida")]
        public string UnidadMedida { get; set; }
        [XmlElement(ElementName = "cantidad")]
        public string Cantidad { get; set; }
        [XmlElement(ElementName = "precioUnitario")]
        public string PrecioUnitario { get; set; }
        [XmlElement(ElementName = "precioSinSubsidio")]
        public string PrecioSinSubsidio { get; set; }
        [XmlElement(ElementName = "descuento")]
        public string Descuento { get; set; }
        [XmlElement(ElementName = "precioTotalSinImpuesto")]
        public string PrecioTotalSinImpuesto { get; set; }
        [XmlElement(ElementName = "detallesAdicionales")]
        public DetallesAdicionalesFactura2 DetallesAdicionales { get; set; }
        [XmlElement(ElementName = "impuestos")]
        public ImpuestosFactura2 Impuestos { get; set; }
    }

    [XmlRoot(ElementName = "detalles")]
    public class DetallesFactura2
    {
        [XmlElement(ElementName = "detalle")]
        public List<DetalleFactura2> Detalle { get; set; }
    }

    [XmlRoot(ElementName = "detalleImpuesto")]
    public class DetalleImpuestoFactura2
    {
        [XmlElement(ElementName = "codigo")]
        public string Codigo { get; set; }
        [XmlElement(ElementName = "codigoPorcentaje")]
        public string CodigoPorcentaje { get; set; }
        [XmlElement(ElementName = "tarifa")]
        public string Tarifa { get; set; }
        [XmlElement(ElementName = "baseImponibleReembolso")]
        public string BaseImponibleReembolso { get; set; }
        [XmlElement(ElementName = "impuestoReembolso")]
        public string ImpuestoReembolso { get; set; }
    }

    [XmlRoot(ElementName = "detalleImpuestos")]
    public class DetalleImpuestosFactura2
    {
        [XmlElement(ElementName = "detalleImpuesto")]
        public List<DetalleImpuestoFactura2> DetalleImpuesto { get; set; }
    }

    [XmlRoot(ElementName = "compensacionReembolso")]
    public class CompensacionReembolsoFactura2
    {
        [XmlElement(ElementName = "codigo")]
        public string Codigo { get; set; }
        [XmlElement(ElementName = "tarifa")]
        public string Tarifa { get; set; }
        [XmlElement(ElementName = "valor")]
        public string Valor { get; set; }
    }

    [XmlRoot(ElementName = "compensacionesReembolso")]
    public class CompensacionesReembolsoFactura2
    {
        [XmlElement(ElementName = "compensacionReembolso")]
        public List<CompensacionReembolsoFactura2> CompensacionReembolso { get; set; }
    }

    [XmlRoot(ElementName = "reembolsoDetalle")]
    public class ReembolsoDetalleFactura2
    {
        [XmlElement(ElementName = "tipoIdentificacionProveedorReembolso")]
        public string TipoIdentificacionProveedorReembolso { get; set; }
        [XmlElement(ElementName = "identificacionProveedorReembolso")]
        public string IdentificacionProveedorReembolso { get; set; }
        [XmlElement(ElementName = "codPaisPagoProveedorReembolso")]
        public string CodPaisPagoProveedorReembolso { get; set; }
        [XmlElement(ElementName = "tipoProveedorReembolso")]
        public string TipoProveedorReembolso { get; set; }
        [XmlElement(ElementName = "codDocReembolso")]
        public string CodDocReembolso { get; set; }
        [XmlElement(ElementName = "estabDocReembolso")]
        public string EstabDocReembolso { get; set; }
        [XmlElement(ElementName = "ptoEmiDocReembolso")]
        public string PtoEmiDocReembolso { get; set; }
        [XmlElement(ElementName = "secuencialDocReembolso")]
        public string SecuencialDocReembolso { get; set; }
        [XmlElement(ElementName = "fechaEmisionDocReembolso")]
        public string FechaEmisionDocReembolso { get; set; }
        [XmlElement(ElementName = "numeroautorizacionDocReemb")]
        public string NumeroautorizacionDocReemb { get; set; }
        [XmlElement(ElementName = "detalleImpuestos")]
        public DetalleImpuestosFactura2 DetalleImpuestos { get; set; }
        [XmlElement(ElementName = "compensacionesReembolso")]
        public CompensacionesReembolsoFactura2 CompensacionesReembolso { get; set; }
    }

    [XmlRoot(ElementName = "reembolsos")]
    public class ReembolsosFactura2
    {
        [XmlElement(ElementName = "reembolsoDetalle")]
        public List<ReembolsoDetalleFactura2> ReembolsoDetalle { get; set; }
    }

    [XmlRoot(ElementName = "retencion")]
    public class RetencionFactura2
    {
        [XmlElement(ElementName = "codigo")]
        public string Codigo { get; set; }
        [XmlElement(ElementName = "codigoPorcentaje")]
        public string CodigoPorcentaje { get; set; }
        [XmlElement(ElementName = "tarifa")]
        public string Tarifa { get; set; }
        [XmlElement(ElementName = "valor")]
        public string Valor { get; set; }
    }

    [XmlRoot(ElementName = "retenciones")]
    public class RetencionesFactura2
    {
        [XmlElement(ElementName = "retencion")]
        public List<RetencionFactura2> Retencion { get; set; }
    }

    [XmlRoot(ElementName = "destino")]
    public class DestinoFactura2
    {
        [XmlElement(ElementName = "motivoTraslado")]
        public string MotivoTraslado { get; set; }
        [XmlElement(ElementName = "docAduaneroUnico")]
        public string DocAduaneroUnico { get; set; }
        [XmlElement(ElementName = "codEstabDestino")]
        public string CodEstabDestino { get; set; }
        [XmlElement(ElementName = "ruta")]
        public string Ruta { get; set; }
    }

    [XmlRoot(ElementName = "destinos")]
    public class DestinosFactura2
    {
        [XmlElement(ElementName = "destino")]
        public List<DestinoFactura2> Destino { get; set; }
    }

    [XmlRoot(ElementName = "infoSustitutivaGuiaRemision")]
    public class InfoSustitutivaGuiaRemisionFactura2
    {
        [XmlElement(ElementName = "dirPartida")]
        public string DirPartida { get; set; }
        [XmlElement(ElementName = "dirDestinatario")]
        public string DirDestinatario { get; set; }
        [XmlElement(ElementName = "fechaIniTransporte")]
        public string FechaIniTransporte { get; set; }
        [XmlElement(ElementName = "fechaFinTransporte")]
        public string FechaFinTransporte { get; set; }
        [XmlElement(ElementName = "razonSocialTransportista")]
        public string RazonSocialTransportista { get; set; }
        [XmlElement(ElementName = "tipoIdentificacionTransportista")]
        public string TipoIdentificacionTransportista { get; set; }
        [XmlElement(ElementName = "rucTransportista")]
        public string RucTransportista { get; set; }
        [XmlElement(ElementName = "placa")]
        public string Placa { get; set; }
        [XmlElement(ElementName = "destinos")]
        public DestinosFactura2 Destinos { get; set; }
    }

    [XmlRoot(ElementName = "rubro")]
    public class RubroFactura2
    {
        [XmlElement(ElementName = "concepto")]
        public string Concepto { get; set; }
        [XmlElement(ElementName = "total")]
        public string Total { get; set; }
    }

    [XmlRoot(ElementName = "otrosRubrosTerceros")]
    public class OtrosRubrosTercerosFactura2
    {
        [XmlElement(ElementName = "rubro")]
        public List<RubroFactura2> Rubro { get; set; }
    }

    [XmlRoot(ElementName = "tipoNegociable")]
    public class TipoNegociableFactura2
    {
        [XmlElement(ElementName = "correo")]
        public string Correo { get; set; }
    }

    [XmlRoot(ElementName = "maquinaFiscal")]
    public class MaquinaFiscalFactura2
    {
        [XmlElement(ElementName = "marca")]
        public string Marca { get; set; }
        [XmlElement(ElementName = "modelo")]
        public string Modelo { get; set; }
        [XmlElement(ElementName = "serie")]
        public string Serie { get; set; }
    }

    [XmlRoot(ElementName = "campoAdicional")]
    public class CampoAdicionalFactura2
    {
        [XmlAttribute(AttributeName = "nombre")]
        public string Nombre { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "infoAdicional")]
    public class InfoAdicionalFactura2
    {
        [XmlElement(ElementName = "campoAdicional")]
        public List<CampoAdicionalFactura2> CampoAdicional { get; set; }
    }

    [XmlRoot(ElementName = "factura")]
    public class Factura2
    {
        [XmlElement(ElementName = "infoTributaria")]
        public InfoTributariaFactura2 InfoTributaria { get; set; }
        [XmlElement(ElementName = "infoFactura")]
        public InfoFacturaFactura2 InfoFactura { get; set; }
        [XmlElement(ElementName = "detalles")]
        public DetallesFactura2 Detalles { get; set; }
        [XmlElement(ElementName = "reembolsos")]
        public ReembolsosFactura2 Reembolsos { get; set; }
        [XmlElement(ElementName = "retenciones")]
        public RetencionesFactura2 Retenciones { get; set; }
        [XmlElement(ElementName = "infoSustitutivaGuiaRemision")]
        public InfoSustitutivaGuiaRemisionFactura2 InfoSustitutivaGuiaRemision { get; set; }
        [XmlElement(ElementName = "otrosRubrosTerceros")]
        public OtrosRubrosTercerosFactura2 OtrosRubrosTerceros { get; set; }
        [XmlElement(ElementName = "tipoNegociable")]
        public TipoNegociableFactura2 TipoNegociable { get; set; }
        [XmlElement(ElementName = "maquinaFiscal")]
        public MaquinaFiscalFactura2 MaquinaFiscal { get; set; }
        [XmlElement(ElementName = "infoAdicional")]
        public InfoAdicionalFactura2 InfoAdicional { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
    }

}
