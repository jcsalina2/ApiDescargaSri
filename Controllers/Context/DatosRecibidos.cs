using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace ApiDescargaSriV9.Context
{

    [Index(nameof(ClaveAccesoAutorizacion), IsUnique = true)]

    public class DatosRecibidos
    {
        //[Required(ErrorMessage = "Campo Requerido")]
        //public string EmpresaApikey { get; set; }

        public int Id { get; set; }
        public int FkEmpresaCon { get; set; }
        [ForeignKey(nameof(FkEmpresaCon))]
        public virtual EmpresaConsulta FkEmpresaConNavigation { get; set; }
        public string Nro { get; set; }

        public string RUCRazonsocialemisor { get; set; }
        public string Tiposeriedecomprobante { get; set; }

        public string ClaveAccesoAutorizacion { get; set; }

        public string Fechahoradeautorizacion { get; set; }

        public string Fechaemision { get; set; }

        public string Tipoemision { get; set; }

        public string Tipoconsulta { get; set; }
        



    }
}
