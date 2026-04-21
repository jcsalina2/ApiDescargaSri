using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace ApiDescargaSriV9.Context
{
    [Index(nameof(EmpresaRuc), IsUnique = true)]
    [Index(nameof(EmpresaApikey), IsUnique = true)]
    public partial class Empresa
    {
        public int Id { get; set; }

        [Column(TypeName = "Varchar(13)")]
        public string EmpresaRuc { get; set; }
        public string EmpresaNombre { get; set; }
        public DateTime EmpresaFechaRegistro { get; set; }
        public DateTime EmpresaFechaFin { get; set; }
        public bool? EmpresaEstado { get; set; }

        public string EmpresaApikey { get; set; }
        public bool? GetSRIElectrnicosRecibido { get; set; }

        public bool? GetSRIElectrnicosEmitidos { get; set; }

        public virtual ICollection<EmpresaConsulta> EmpresaConsultas { get; set; }
    }
}
