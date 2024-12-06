using System;
using System.Collections.Generic;
using ADO.BL.DataEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ADO.Access.DataDev
{
    public partial class DannteDevelopmentContext : DbContext
    {
        public DannteDevelopmentContext()
        {
        }

        public DannteDevelopmentContext(DbContextOptions<DannteDevelopmentContext> options)
            : base(options)
        {
        }

        public virtual DbSet<FileIoTemp> FileIoTemps { get; set; } = null!;
        public virtual DbSet<MpLightning> MpLightnings { get; set; } = null!;
        public virtual DbSet<FileIoTempDetail> FileIoTempDetails { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileIoTemp>(entity =>
            {
                entity.ToTable("file_io_temp");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.ApoyoApertura)
                    .HasMaxLength(50)
                    .HasColumnName("apoyo_apertura");

                entity.Property(e => e.ApoyoFalla)
                    .HasMaxLength(50)
                    .HasColumnName("apoyo_falla");

                entity.Property(e => e.CodInterruptor)
                    .HasMaxLength(50)
                    .HasColumnName("cod_interruptor");

                entity.Property(e => e.CodigoCausaEvento).HasColumnName("codigo_causa_evento");

                entity.Property(e => e.CodigoCircuito)
                    .HasMaxLength(50)
                    .HasColumnName("codigo_circuito");

                entity.Property(e => e.CodigoEvento)
                    .HasMaxLength(50)
                    .HasColumnName("codigo_evento");

                entity.Property(e => e.Duracion).HasColumnName("duracion");

                entity.Property(e => e.FechaFinal).HasColumnName("fecha_final");

                entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");

                entity.Property(e => e.NombreTipoInterruptor)
                    .HasMaxLength(50)
                    .HasColumnName("nombre_tipo_interruptor");

                entity.Property(e => e.TotalClientes).HasColumnName("total_clientes");

                entity.Property(e => e.TotalOperaciones).HasColumnName("total_operaciones");

                entity.Property(e => e.TotalTrafo).HasColumnName("total_trafo");
            });

            modelBuilder.Entity<MpLightning>(entity =>
            {
                entity.ToTable("mp_lightning", "maps");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Amperage).HasColumnName("amperage");

                entity.Property(e => e.DateEvent)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("date_event");

                entity.Property(e => e.Error).HasColumnName("error");

                entity.Property(e => e.Fparent)
                    .HasMaxLength(50)
                    .HasColumnName("fparent")
                    .HasDefaultValueSql("'NO DATA'::character varying");

                entity.Property(e => e.Latitude).HasColumnName("latitude");

                entity.Property(e => e.Longitude).HasColumnName("longitude");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.NameLocality)
                    .HasMaxLength(100)
                    .HasColumnName("name_locality")
                    .HasDefaultValueSql("'NO DATA'::character varying");

                entity.Property(e => e.NameRegion)
                    .HasMaxLength(100)
                    .HasColumnName("name_region")
                    .HasDefaultValueSql("'NO DATA'::character varying");

                entity.Property(e => e.NameZone)
                    .HasMaxLength(100)
                    .HasColumnName("name_zone")
                    .HasDefaultValueSql("'NO DATA'::character varying");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<FileIoTempDetail>(entity =>
            {
                entity.ToTable("file_io_temp_detail");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.ApoyoApertura)
                    .HasMaxLength(50)
                    .HasColumnName("apoyo_apertura");

                entity.Property(e => e.ApoyoFalla)
                    .HasMaxLength(50)
                    .HasColumnName("apoyo_falla");

                entity.Property(e => e.CodInterruptor)
                    .HasMaxLength(50)
                    .HasColumnName("cod_interruptor");

                entity.Property(e => e.CodigoCausaEvento).HasColumnName("codigo_causa_evento");

                entity.Property(e => e.CodigoCircuito)
                    .HasMaxLength(50)
                    .HasColumnName("codigo_circuito");

                entity.Property(e => e.CodigoEvento)
                    .HasMaxLength(50)
                    .HasColumnName("codigo_evento");

                entity.Property(e => e.Duracion).HasColumnName("duracion");

                entity.Property(e => e.FechaFinal).HasColumnName("fecha_final");

                entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");

                entity.Property(e => e.NombreTipoInterruptor)
                    .HasMaxLength(50)
                    .HasColumnName("nombre_tipo_interruptor");

                entity.Property(e => e.TotalClientes).HasColumnName("total_clientes");

                entity.Property(e => e.TotalOperaciones).HasColumnName("total_operaciones");

                entity.Property(e => e.TotalTrafo).HasColumnName("total_trafo");

                entity.Property(e => e.UiaTrafo)
                    .HasMaxLength(50)
                    .HasColumnName("uia_trafo");
            });

            modelBuilder.HasSequence("activity_history_id_seq", "planner");

            modelBuilder.HasSequence("activity_id_seq", "planner");

            modelBuilder.HasSequence("consequence_by_circuit_id_seq", "criticality");

            modelBuilder.HasSequence("consequence_parameter_id_seq", "criticality");

            modelBuilder.HasSequence("consequence_principal_id_seq", "criticality");

            modelBuilder.HasSequence("constructive_unit_id_tb_seq", "criticality");

            modelBuilder.HasSequence("criticality_all_asset_id_seq", "criticality");

            modelBuilder.HasSequence("criticality_all_asset_ind_id_seq", "criticality");

            modelBuilder.HasSequence("criticality_category_id_tb_seq", "criticality");

            modelBuilder.HasSequence("failure_io_exc_id_seq", "criticality");

            modelBuilder.HasSequence("failure_io_id_seq", "criticality");

            modelBuilder.HasSequence("failure_io_monthly_id_seq", "criticality");

            modelBuilder.HasSequence("failure_io_noexc_id_seq", "criticality");

            modelBuilder.HasSequence("failure_io_yearly_id_seq", "criticality");

            modelBuilder.HasSequence("health_all_asset_id_seq", "criticality");

            modelBuilder.HasSequence("historical_failure_id_seq", "criticality");

            modelBuilder.HasSequence("resume_current_frequency_by_cause_id_seq", "criticality");

            modelBuilder.HasSequence("resume_frequency_map_id_seq", "criticality");

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
