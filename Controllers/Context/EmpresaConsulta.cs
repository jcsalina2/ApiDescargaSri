using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiDescargaSriV9.Context
{
    [Microsoft.EntityFrameworkCore.Index(nameof(EmpresaRuc), IsUnique = true)]
    public class EmpresaConsulta
    {
        public int Id { get; set; }
        public int FkEmpresa { get; set; }
        [ForeignKey(nameof(FkEmpresa))]
        public virtual Empresa FkEmpresaNavigation { get; set; }

        [Column(TypeName = "Varchar(13)")]
        public string EmpresaRuc { get; set; }
  
        public virtual ICollection<DatosRecibidos> DatosRecibidos { get; set; }

    }
}
