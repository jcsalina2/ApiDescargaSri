using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;


#nullable disable

namespace ApiDescargaSriV9.Context
{
    public partial class AplicationDbContext : DbContext
    {
        public AplicationDbContext(DbContextOptions<AplicationDbContext> options)
          : base(options)
        {

        }
        public virtual DbSet<Empresa> Empresas { get; set; }
        public virtual DbSet<EmpresaConsulta> EmpresaConsultas { get; set; }

        public virtual DbSet<DatosRecibidos> DatosRecibidos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                //ptionsBuilder.UseSqlServer("Server=EDWIN-PC\\SQLEXPRESS;Database=DBFactElecNesSoft;Trusted_Connection=True;");

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");     
            OnModelCreatingPartial(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
