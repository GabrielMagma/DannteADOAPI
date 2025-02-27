using ADO.BL.DataEntities;
using Microsoft.EntityFrameworkCore;

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
        public virtual DbSet<FilesLac> FilesLacs { get; set; } = null!;
        public virtual DbSet<FilesTc1> FilesTc1s { get; set; } = null!;
        public virtual DbSet<FilesTt2> FilesTt2s { get; set; } = null!;
        public virtual DbSet<AllAsset> AllAssets { get; set; } = null!;
        public virtual DbSet<FilesIo> FilesIos { get; set; } = null!;
        public virtual DbSet<Poda> Podas { get; set; } = null!;


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

            modelBuilder.Entity<FilesLac>(entity =>
            {
                entity.ToTable("files_lac", "machine");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasIdentityOptions(null, null, 0L);

                entity.Property(e => e.AffectsConnection)
                    .HasColumnName("affects_connection")
                    .HasDefaultValueSql("'-1'::integer");

                entity.Property(e => e.ElementType)
                    .HasColumnName("element_type")
                    .HasDefaultValueSql("'-1'::integer");

                entity.Property(e => e.EndDate)
                    .HasColumnType("timestamp(3) without time zone")
                    .HasColumnName("end_date");

                entity.Property(e => e.EventCause)
                    .HasColumnName("event_cause")
                    .HasDefaultValueSql("'-1'::integer");

                entity.Property(e => e.EventCode)
                    .HasMaxLength(52)
                    .HasColumnName("event_code")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.EventContinues)
                    .HasMaxLength(2)
                    .HasColumnName("event_continues")
                    .HasDefaultValueSql("'-1'::character varying");

                entity.Property(e => e.EventExcluidZin)
                    .HasColumnName("event_excluid_zin")
                    .HasDefaultValueSql("'-1'::integer");

                entity.Property(e => e.FileCode)
                    .HasMaxLength(52)
                    .HasColumnName("file_code")
                    .HasDefaultValueSql("'NO DATA'::character varying");

                entity.Property(e => e.Files)
                    .HasMaxLength(52)
                    .HasColumnName("files")
                    .HasDefaultValueSql("'-1'::character varying");

                entity.Property(e => e.FilesDate)
                    .HasColumnName("files_date")
                    .HasDefaultValueSql("CURRENT_DATE");

                entity.Property(e => e.Fparent)
                    .HasMaxLength(16)
                    .HasColumnName("fparent")
                    .HasDefaultValueSql("'-1'::character varying");

                entity.Property(e => e.LightingUsers)
                    .HasColumnName("lighting_users")
                    .HasDefaultValueSql("'-1'::integer");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.StartDate)
                    .HasColumnType("timestamp(3) without time zone")
                    .HasColumnName("start_date");

                entity.Property(e => e.State)
                    .HasColumnName("state")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.Uia)
                    .HasMaxLength(52)
                    .HasColumnName("uia")
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<FilesTc1>(entity =>
            {
                entity.ToTable("files_tc1");

                entity.HasIndex(e => new { e.Month, e.Year }, "idx_files_tc1_month_year");

                entity.HasIndex(e => e.Uia, "idx_files_tc1_uia");

                entity.HasIndex(e => new { e.Uia, e.Niu, e.Year, e.Month }, "idx_files_tc1_uia_niuyearmonth");

                entity.HasIndex(e => new { e.Year, e.Month }, "idx_files_tc1_year_month");

                entity.HasIndex(e => new { e.Year, e.Month }, "idx_year_month");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn()
                    .HasIdentityOptions(21204935L);

                entity.Property(e => e.Files)
                    .HasMaxLength(52)
                    .HasColumnName("files")
                    .HasDefaultValueSql("'-1'::character varying");

                entity.Property(e => e.FilesDate).HasColumnName("files_date");

                entity.Property(e => e.Month)
                    .HasColumnName("month")
                    .HasDefaultValueSql("'-1'::integer");

                entity.Property(e => e.Niu)
                    .HasMaxLength(52)
                    .HasColumnName("niu");

                entity.Property(e => e.Uia)
                    .HasMaxLength(52)
                    .HasColumnName("uia");

                entity.Property(e => e.Year)
                    .HasColumnName("year")
                    .HasDefaultValueSql("'-1'::integer");
            });

            modelBuilder.Entity<FilesTt2>(entity =>
            {
                entity.ToTable("files_tt2");

                entity.HasIndex(e => e.CodeSig, "idx_files_tt2_code_sig");

                entity.HasIndex(e => e.Uia, "idx_files_tt2_uia");

                entity.HasIndex(e => new { e.Uia, e.Year, e.Month }, "idx_files_tt2_uia_year_month");

                entity.HasIndex(e => e.Uia, "indx_uia_tt2");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CodeSig)
                    .HasMaxLength(50)
                    .HasColumnName("code_sig");

                entity.Property(e => e.Files)
                    .HasMaxLength(52)
                    .HasColumnName("files");

                entity.Property(e => e.FilesDate).HasColumnName("files_date");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.State)
                    .HasColumnName("state")
                    .HasDefaultValueSql("2");

                entity.Property(e => e.StateDate).HasColumnName("state_date");

                entity.Property(e => e.Uia)
                    .HasMaxLength(50)
                    .HasColumnName("uia");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<AllAsset>(entity =>
            {
                entity.ToTable("all_asset");

                entity.HasIndex(e => new { e.Uia, e.IdRegion, e.IdLocality }, "idx_all_asset");

                entity.HasIndex(e => new { e.CodeSig, e.Id }, "idx_all_asset_code_sig_id");

                entity.HasIndex(e => new { e.IdLocality, e.IdRegion }, "idx_all_asset_id_locality_region");

                entity.HasIndex(e => e.Uia, "idx_all_asset_uia");

                entity.HasIndex(e => e.CodeSig, "index_code_sig_spard_all");

                entity.HasIndex(e => new { e.CodeSig, e.Uia }, "unique_code_sig_uia")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(200)
                    .HasColumnName("address")
                    .HasDefaultValueSql("'-1'::character varying")
                    .HasComment("Dirección de la ubicación del activo");

                entity.Property(e => e.CodeSig)
                    .HasMaxLength(32)
                    .HasColumnName("code_sig")
                    .HasDefaultValueSql("'-1'::character varying")
                    .HasComment("Código de identificación del activo dentro del circuito.\nDepende de la posición geográfica y no varía si se reemplaza el activo");

                entity.Property(e => e.Codetaxo)
                    .HasMaxLength(32)
                    .HasColumnName("codetaxo")
                    .HasDefaultValueSql("'-1'::character varying")
                    .HasComment("Código taxonómico; utilizado para identificar el activo en el software Máximo");

                entity.Property(e => e.DateInst)
                    .HasColumnName("date_inst")
                    .HasComment("Fecha de instalación del activo");

                entity.Property(e => e.DateUnin)
                    .HasColumnName("date_unin")
                    .HasComment("Fecha de desinstalación de activo");

                entity.Property(e => e.Fparent)
                    .HasMaxLength(16)
                    .HasColumnName("fparent")
                    .HasDefaultValueSql("'-1'::character varying")
                    .HasComment("Código del circuito al cual pertenece el activo");

                entity.Property(e => e.GeographicalCode).HasColumnName("geographical_code");

                entity.Property(e => e.Group015)
                    .HasMaxLength(2)
                    .HasColumnName("group015")
                    .HasDefaultValueSql("'-1'::character varying")
                    .HasComment("Grupo de calidad del activo; de acuerdo a la CREG 015\n\nPrimer Dígito: Criticidad del Activo\n1: Alta Criticidad\n2: Media Criticidad\n3: Baja Criticidad\n\nSegundo Dígito: Nivel de Tensión\n1: Alta Tensión (AT)\n2: Media Tensión (MT)\n3: Baja Tensión (BT)");

                entity.Property(e => e.IdLocality).HasColumnName("id_locality");

                entity.Property(e => e.IdRegion).HasColumnName("id_region");

                entity.Property(e => e.IdSector).HasColumnName("id_sector");

                entity.Property(e => e.IdZone).HasColumnName("id_zone");

                entity.Property(e => e.Latitude)
                    .HasColumnName("latitude")
                    .HasDefaultValueSql("0")
                    .HasComment("Latitud; ubicación geográfica del activo");

                entity.Property(e => e.Longitude)
                    .HasColumnName("longitude")
                    .HasDefaultValueSql("0")
                    .HasComment("Longitud; ubicación geográfica del activo");

                entity.Property(e => e.NameLocality)
                    .HasMaxLength(100)
                    .HasColumnName("name_locality");

                entity.Property(e => e.NameRegion)
                    .HasMaxLength(100)
                    .HasColumnName("name_region");

                entity.Property(e => e.NameSector)
                    .HasMaxLength(100)
                    .HasColumnName("name_sector");

                entity.Property(e => e.NameZone)
                    .HasMaxLength(100)
                    .HasColumnName("name_zone");

                entity.Property(e => e.Poblation)
                    .HasMaxLength(2)
                    .HasColumnName("poblation")
                    .HasDefaultValueSql("'-1'::character varying");

                entity.Property(e => e.State)
                    .HasColumnName("state")
                    .HasDefaultValueSql("2");

                entity.Property(e => e.TypeAsset)
                    .HasMaxLength(32)
                    .HasColumnName("type_asset")
                    .HasDefaultValueSql("'-1'::character varying")
                    .HasComment("Tipo de activo: transformador, interruptor, reconectador o seccionador");

                entity.Property(e => e.Uccap14)
                    .HasMaxLength(6)
                    .HasColumnName("uccap14")
                    .HasDefaultValueSql("'-1'::character varying")
                    .HasComment("Unidad constructiva del activo;  de acuerdo al capítulo 14 de la CREG 015.\n\nEs una agrupación lógica de componentes que funcionan juntos como una unidad única y que tienen un propósito específico dentro del sistema eléctrico.\n\nEn DANNTE lo utilizamos para identificar el tiempo de vida util de un activo de acuerdo a la CREG");

                entity.Property(e => e.Uia)
                    .HasMaxLength(50)
                    .HasColumnName("uia")
                    .HasDefaultValueSql("'-1'::character varying")
                    .HasComment("Código de identificación del activo; está asociado al code_sig\nVaría si se reemplaza el activo");
            });

            modelBuilder.Entity<FilesIo>(entity =>
            {
                entity.HasKey(e => e.IdTb)
                    .HasName("files_io_pkey");

                entity.ToTable("files_io");

                entity.Property(e => e.IdTb)
                    .HasColumnName("id_tb")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Cause).HasColumnName("cause");

                entity.Property(e => e.CodeSig)
                    .HasMaxLength(50)
                    .HasColumnName("code_sig");

                entity.Property(e => e.Component)
                    .HasMaxLength(200)
                    .HasColumnName("component");

                entity.Property(e => e.CregCause).HasColumnName("creg_cause");

                entity.Property(e => e.DateIo).HasColumnName("date_io");

                entity.Property(e => e.Dependence)
                    .HasMaxLength(50)
                    .HasColumnName("dependence");

                entity.Property(e => e.DnaKwh).HasColumnName("dna_kwh");

                entity.Property(e => e.Element)
                    .HasMaxLength(200)
                    .HasColumnName("element");

                entity.Property(e => e.EventType)
                    .HasMaxLength(50)
                    .HasColumnName("event_type");

                entity.Property(e => e.Failure).HasColumnName("failure");

                entity.Property(e => e.FileIo)
                    .HasMaxLength(50)
                    .HasColumnName("file_io");

                entity.Property(e => e.FilesDate)
                    .HasColumnName("files_date")
                    .HasDefaultValueSql("CURRENT_DATE");

                entity.Property(e => e.Fparent)
                    .HasMaxLength(50)
                    .HasColumnName("fparent");

                entity.Property(e => e.HourIn)
                    .HasColumnType("timestamp(6) without time zone")
                    .HasColumnName("hour_in");

                entity.Property(e => e.HourInterruption).HasColumnName("hour_interruption");

                entity.Property(e => e.HourOut)
                    .HasColumnType("timestamp(6) without time zone")
                    .HasColumnName("hour_out");

                entity.Property(e => e.Maneuver)
                    .HasMaxLength(50)
                    .HasColumnName("maneuver");

                entity.Property(e => e.MinInterruption).HasColumnName("min_interruption");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.TypeAsset)
                    .HasMaxLength(20)
                    .HasColumnName("type_asset");

                entity.Property(e => e.Users).HasColumnName("users");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<Poda>(entity =>
            {
                entity.ToTable("podas", "machine");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.BeginSup).HasMaxLength(100);

                entity.Property(e => e.Circuit).HasMaxLength(100);

                entity.Property(e => e.Consig).HasMaxLength(100);

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.EndSup).HasMaxLength(100);

                entity.Property(e => e.Item).HasMaxLength(100);

                entity.Property(e => e.NameLocation).HasMaxLength(100);

                entity.Property(e => e.NameRegion).HasMaxLength(100);

                entity.Property(e => e.NameZone).HasMaxLength(100);

                entity.Property(e => e.NoOt)
                    .HasMaxLength(100)
                    .HasColumnName("NoOT");

                entity.Property(e => e.NoReport).HasMaxLength(100);

                entity.Property(e => e.Pqr)
                    .HasMaxLength(100)
                    .HasColumnName("PQR");

                entity.Property(e => e.Scheduled).HasMaxLength(100);

                entity.Property(e => e.StateOt)
                    .HasMaxLength(100)
                    .HasColumnName("StateOT");

                entity.Property(e => e.Urban).HasMaxLength(2);
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
