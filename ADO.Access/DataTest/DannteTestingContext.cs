using ADO.BL.DataEntities;
using Microsoft.EntityFrameworkCore;

namespace ADO.Access.DataTest
{
    public partial class DannteTestingContext : DbContext
    {
        public DannteTestingContext()
        {
        }

        public DannteTestingContext(DbContextOptions<DannteTestingContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AllAsset> AllAssets { get; set; } = null!;
        public virtual DbSet<AllAssetEep> AllAssetEeps { get; set; } = null!;
        public virtual DbSet<FilesIo> FilesIos { get; set; } = null!;
        public virtual DbSet<IaIdeam> IaIdeams { get; set; } = null!;
        public virtual DbSet<IaPoda> IaPodas { get; set; } = null!;
        public virtual DbSet<MpLightning> MpLightnings { get; set; } = null!;
        public virtual DbSet<MpUtilityPole> MpUtilityPoles { get; set; } = null!;
        public virtual DbSet<MpCompensation> MpCompensations { get; set; } = null!;
        public virtual DbSet<FilesIoComplete> FilesIoCompletes { get; set; } = null!;
        public virtual DbSet<IoComment> IoComments { get; set; } = null!;
        public virtual DbSet<FileIoTemp> FileIoTemps { get; set; } = null!;        
        public virtual DbSet<FileIoTempDetail> FileIoTempDetails { get; set; } = null!;
        public virtual DbSet<QueueStatusAsset> QueueStatusAssets { get; set; } = null!;
        public virtual DbSet<QueueStatusIo> QueueStatusIos { get; set; } = null!;
        public virtual DbSet<QueueStatusLac> QueueStatusLacs { get; set; } = null!;
        public virtual DbSet<QueueStatusSspd> QueueStatusSspds { get; set; } = null!;
        public virtual DbSet<QueueStatusTc1> QueueStatusTc1s { get; set; } = null!;
        public virtual DbSet<QueueStatusTt2> QueueStatusTt2s { get; set; } = null!;
        public virtual DbSet<QueueStatusPole> QueueStatusPoles { get; set; } = null!;
        public virtual DbSet<QueueStatusLightning> QueueStatusLightnings { get; set; } = null!;
        public virtual DbSet<QueueStatusCompensation> QueueStatusCompensations { get; set; } = null!;
        public virtual DbSet<QueueStatusPoda> QueueStatusPodas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("dblink");

            modelBuilder.Entity<AllAsset>(entity =>
            {
                entity.ToTable("all_asset");

                entity.HasIndex(e => new { e.CodeSig, e.Id }, "idx_all_asset_code_sig_id");

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

                entity.Property(e => e.IdLocality)
                    .HasColumnName("id_locality")
                    .HasDefaultValueSql("'-1'::integer");

                entity.Property(e => e.IdRegion)
                    .HasColumnName("id_region")
                    .HasDefaultValueSql("0");

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

                entity.Property(e => e.Month)
                    .HasColumnName("month")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.NameLocality)
                    .HasMaxLength(100)
                    .HasColumnName("name_locality");

                entity.Property(e => e.NameRegion)
                    .HasMaxLength(100)
                    .HasColumnName("name_region")
                    .HasDefaultValueSql("'GENERAL'::character varying");

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

                entity.Property(e => e.Year)
                    .HasColumnName("year")
                    .HasDefaultValueSql("0");
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

            modelBuilder.Entity<IaIdeam>(entity =>
            {
                entity.ToTable("ia_ideam", "machine");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('machine.ideam_id_seq'::regclass)");

                entity.Property(e => e.Altitude).HasColumnName("altitude");

                entity.Property(e => e.Date).HasColumnName("date");

                entity.Property(e => e.Department)
                    .HasMaxLength(255)
                    .HasColumnName("department");

                entity.Property(e => e.Frequency)
                    .HasMaxLength(20)
                    .HasColumnName("frequency");

                entity.Property(e => e.Latitude).HasColumnName("latitude");

                entity.Property(e => e.Longitude).HasColumnName("longitude");

                entity.Property(e => e.Municipality)
                    .HasMaxLength(255)
                    .HasColumnName("municipality");

                entity.Property(e => e.Parameterid)
                    .HasMaxLength(20)
                    .HasColumnName("parameterid");

                entity.Property(e => e.Precipitation).HasColumnName("precipitation");

                entity.Property(e => e.Stationcode)
                    .HasMaxLength(10)
                    .HasColumnName("stationcode");

                entity.Property(e => e.Stationname)
                    .HasMaxLength(255)
                    .HasColumnName("stationname");
            });

            modelBuilder.Entity<IaPoda>(entity =>
            {
                entity.ToTable("ia_podas", "machine");

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

            modelBuilder.Entity<MpUtilityPole>(entity =>
            {
                entity.ToTable("mp_utility_pole", "maps");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Fparent)
                    .HasMaxLength(20)
                    .HasColumnName("fparent");                

                entity.Property(e => e.InventaryCode)
                    .HasMaxLength(50)
                    .HasColumnName("inventary_code");

                entity.Property(e => e.Latitude).HasColumnName("latitude");

                entity.Property(e => e.Longitude).HasColumnName("longitude");

                entity.Property(e => e.PaintingCode)
                    .HasMaxLength(50)
                    .HasColumnName("painting_code");

                entity.Property(e => e.TypePole).HasColumnName("type_pole");
                
            });

            modelBuilder.Entity<MpCompensation>(entity =>
            {
                entity.ToTable("mp_compensation", "maps");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('maps.compensation_id_seq'::regclass)");

                entity.Property(e => e.CodeSig)
                    .HasMaxLength(32)
                    .HasColumnName("code_sig");

                entity.Property(e => e.Fparent)
                    .HasMaxLength(16)
                    .HasColumnName("fparent");

                entity.Property(e => e.Latitude).HasColumnName("latitude");

                entity.Property(e => e.Longitude).HasColumnName("longitude");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.Nui)
                    .HasMaxLength(255)
                    .HasColumnName("nui");

                entity.Property(e => e.QualityGroup)
                    .HasMaxLength(255)
                    .HasColumnName("quality_group");

                entity.Property(e => e.TensionLevel)
                    .HasMaxLength(255)
                    .HasColumnName("tension_level");

                entity.Property(e => e.Vc).HasColumnName("vc");

                entity.Property(e => e.Vcd).HasColumnName("vcd");

                entity.Property(e => e.Vcf).HasColumnName("vcf");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<FilesIoComplete>(entity =>
            {
                entity.ToTable("files_io_complete");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.AffectedSector).HasColumnName("affected_sector");

                entity.Property(e => e.ApplicationId)
                    .HasMaxLength(50)
                    .HasColumnName("application_id");

                entity.Property(e => e.CapacityKva).HasColumnName("capacity_kva");

                entity.Property(e => e.Cause).HasColumnName("cause");

                entity.Property(e => e.CodCauseEvent).HasColumnName("cod_cause_event");

                entity.Property(e => e.CodeConsig).HasColumnName("code_consig");

                entity.Property(e => e.CodeGis)
                    .HasMaxLength(50)
                    .HasColumnName("code_gis");

                entity.Property(e => e.Component)
                    .HasMaxLength(50)
                    .HasColumnName("component");

                entity.Property(e => e.DateIo).HasColumnName("date_io");

                entity.Property(e => e.Dependency)
                    .HasMaxLength(50)
                    .HasColumnName("dependency");

                entity.Property(e => e.DescCause)
                    .HasMaxLength(50)
                    .HasColumnName("desc_cause");

                entity.Property(e => e.DnaKwh).HasColumnName("dna_kwh");

                entity.Property(e => e.Element)
                    .HasMaxLength(50)
                    .HasColumnName("element");

                entity.Property(e => e.FuseCap)
                    .HasMaxLength(50)
                    .HasColumnName("fuse_cap");

                entity.Property(e => e.FuseQuant)
                    .HasMaxLength(50)
                    .HasColumnName("fuse_quant");

                entity.Property(e => e.HourIn)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("hour_in");

                entity.Property(e => e.HourInterruption).HasColumnName("hour_interruption");

                entity.Property(e => e.HourOut)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("hour_out");

                entity.Property(e => e.Location)
                    .HasMaxLength(50)
                    .HasColumnName("location");

                entity.Property(e => e.Maneuver)
                    .HasMaxLength(50)
                    .HasColumnName("maneuver");

                entity.Property(e => e.MinInterruption).HasColumnName("min_interruption");

                entity.Property(e => e.Observation).HasColumnName("observation");

                entity.Property(e => e.OutPower).HasColumnName("out_power");

                entity.Property(e => e.Ownership)
                    .HasMaxLength(50)
                    .HasColumnName("ownership");

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .HasColumnName("type");

                entity.Property(e => e.TypeEvent)
                    .HasMaxLength(50)
                    .HasColumnName("type_event");

                entity.Property(e => e.Ubication)
                    .HasMaxLength(50)
                    .HasColumnName("ubication");

                entity.Property(e => e.Users).HasColumnName("users");
            });

            modelBuilder.Entity<IoComment>(entity =>
            {
                entity.ToTable("io_comments");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.AffectedSector)
                    .HasMaxLength(3072)
                    .HasColumnName("affected_sector");

                entity.Property(e => e.Comment)
                    .HasMaxLength(3072)
                    .HasColumnName("comment");

                entity.Property(e => e.FileLine).HasColumnName("file_line");

                entity.Property(e => e.FileName)
                    .HasMaxLength(50)
                    .HasColumnName("file_name");
            });

            modelBuilder.Entity<FileIoTempDetail>(entity =>
            {
                entity.ToTable("file_io_temp_detail");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.BeginDate).HasColumnName("begin_date");

                entity.Property(e => e.CodeCauseEvent).HasColumnName("code_cause_event");

                entity.Property(e => e.CodeEvent)
                    .HasMaxLength(50)
                    .HasColumnName("code_event");

                entity.Property(e => e.CodeSwitch)
                    .HasMaxLength(50)
                    .HasColumnName("code_switch");

                entity.Property(e => e.Duration).HasColumnName("duration");

                entity.Property(e => e.EndDate).HasColumnName("end_date");

                entity.Property(e => e.Fparent)
                    .HasMaxLength(50)
                    .HasColumnName("fparent");

                entity.Property(e => e.NameTypeSwitch)
                    .HasMaxLength(50)
                    .HasColumnName("name_type_switch");

                entity.Property(e => e.SupportFailure)
                    .HasMaxLength(50)
                    .HasColumnName("support_failure");

                entity.Property(e => e.SupportOpen)
                    .HasMaxLength(50)
                    .HasColumnName("support_open");

                entity.Property(e => e.TotalClients).HasColumnName("total_clients");

                entity.Property(e => e.TotalOperations).HasColumnName("total_operations");

                entity.Property(e => e.TotalTrafo).HasColumnName("total_trafo");

                entity.Property(e => e.UiaTrafo)
                    .HasMaxLength(50)
                    .HasColumnName("uia_trafo");
            });

            modelBuilder.Entity<AllAssetEep>(entity =>
            {
                entity.ToTable("all_asset_eep");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Address)
                    .HasMaxLength(200)
                    .HasColumnName("address")
                    .HasDefaultValueSql("'-1'::character varying");

                entity.Property(e => e.CodeSig)
                    .HasMaxLength(32)
                    .HasColumnName("code_sig")
                    .HasDefaultValueSql("'-1'::character varying");

                entity.Property(e => e.Codetaxo)
                    .HasMaxLength(32)
                    .HasColumnName("codetaxo")
                    .HasDefaultValueSql("'-1'::character varying");

                entity.Property(e => e.DateInst).HasColumnName("date_inst");

                entity.Property(e => e.DateUnin).HasColumnName("date_unin");

                entity.Property(e => e.Fparent)
                    .HasMaxLength(16)
                    .HasColumnName("fparent")
                    .HasDefaultValueSql("'-1'::character varying");

                entity.Property(e => e.GeographicalCode).HasColumnName("geographical_code");

                entity.Property(e => e.Group015)
                    .HasMaxLength(2)
                    .HasColumnName("group015")
                    .HasDefaultValueSql("'-1'::character varying");

                entity.Property(e => e.IdLocality).HasColumnName("id_locality");

                entity.Property(e => e.IdRegion).HasColumnName("id_region");

                entity.Property(e => e.IdSector).HasColumnName("id_sector");

                entity.Property(e => e.IdZone).HasColumnName("id_zone");

                entity.Property(e => e.Latitude)
                    .HasColumnName("latitude")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.Longitude)
                    .HasColumnName("longitude")
                    .HasDefaultValueSql("0");

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
                    .HasDefaultValueSql("'-1'::character varying");

                entity.Property(e => e.Uccap14)
                    .HasMaxLength(6)
                    .HasColumnName("uccap14")
                    .HasDefaultValueSql("'-1'::character varying");

                entity.Property(e => e.Uia)
                    .HasMaxLength(50)
                    .HasColumnName("uia")
                    .HasDefaultValueSql("'-1'::character varying");
            });

            modelBuilder.Entity<FileIoTemp>(entity =>
            {
                entity.ToTable("file_io_temp");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.BeginDate).HasColumnName("begin_date");

                entity.Property(e => e.CodeCauseEvent).HasColumnName("code_cause_event");

                entity.Property(e => e.CodeEvent)
                    .HasMaxLength(50)
                    .HasColumnName("code_event");

                entity.Property(e => e.CodeSwitch)
                    .HasMaxLength(50)
                    .HasColumnName("code_switch");

                entity.Property(e => e.Duration).HasColumnName("duration");

                entity.Property(e => e.EndDate).HasColumnName("end_date");

                entity.Property(e => e.Fparent)
                    .HasMaxLength(50)
                    .HasColumnName("fparent");

                entity.Property(e => e.NameTypeSwitch)
                    .HasMaxLength(50)
                    .HasColumnName("name_type_switch");

                entity.Property(e => e.SupportFailure)
                    .HasMaxLength(50)
                    .HasColumnName("support_failure");

                entity.Property(e => e.SupportOpen)
                    .HasMaxLength(50)
                    .HasColumnName("support_open");

                entity.Property(e => e.TotalClients).HasColumnName("total_clients");

                entity.Property(e => e.TotalOperations).HasColumnName("total_operations");

                entity.Property(e => e.TotalTrafo).HasColumnName("total_trafo");
            });

            modelBuilder.Entity<QueueStatusAsset>(entity =>
            {
                entity.ToTable("queue_status_asset", "queues");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.DateFile).HasColumnName("date_file");

                entity.Property(e => e.DateRegister).HasColumnName("date_register");

                entity.Property(e => e.Day).HasColumnName("day");

                entity.Property(e => e.FileName)
                    .HasMaxLength(50)
                    .HasColumnName("file_name");

                entity.Property(e => e.FileType)
                    .HasMaxLength(50)
                    .HasColumnName("file_type");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<QueueStatusIo>(entity =>
            {
                entity.ToTable("queue_status_io", "queues");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.DateFile).HasColumnName("date_file");

                entity.Property(e => e.DateRegister).HasColumnName("date_register");

                entity.Property(e => e.Day).HasColumnName("day");

                entity.Property(e => e.FileName)
                    .HasMaxLength(50)
                    .HasColumnName("file_name");

                entity.Property(e => e.FileType)
                    .HasMaxLength(50)
                    .HasColumnName("file_type");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<QueueStatusLac>(entity =>
            {
                entity.ToTable("queue_status_lac", "queues");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.DateFile).HasColumnName("date_file");

                entity.Property(e => e.DateRegister).HasColumnName("date_register");

                entity.Property(e => e.Day).HasColumnName("day");

                entity.Property(e => e.FileName)
                    .HasMaxLength(50)
                    .HasColumnName("file_name");

                entity.Property(e => e.FileType)
                    .HasMaxLength(50)
                    .HasColumnName("file_type");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<QueueStatusSspd>(entity =>
            {
                entity.ToTable("queue_status_sspd", "queues");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.DateFile).HasColumnName("date_file");

                entity.Property(e => e.DateRegister).HasColumnName("date_register");

                entity.Property(e => e.Day).HasColumnName("day");

                entity.Property(e => e.FileName)
                    .HasMaxLength(50)
                    .HasColumnName("file_name");

                entity.Property(e => e.FileType)
                    .HasMaxLength(50)
                    .HasColumnName("file_type");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<QueueStatusTc1>(entity =>
            {
                entity.ToTable("queue_status_tc1", "queues");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.DateFile).HasColumnName("date_file");

                entity.Property(e => e.DateRegister).HasColumnName("date_register");

                entity.Property(e => e.Day).HasColumnName("day");

                entity.Property(e => e.FileName)
                    .HasMaxLength(50)
                    .HasColumnName("file_name");

                entity.Property(e => e.FileType)
                    .HasMaxLength(50)
                    .HasColumnName("file_type");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<QueueStatusTt2>(entity =>
            {
                entity.ToTable("queue_status_tt2", "queues");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.DateFile).HasColumnName("date_file");

                entity.Property(e => e.DateRegister).HasColumnName("date_register");

                entity.Property(e => e.Day).HasColumnName("day");

                entity.Property(e => e.FileName)
                    .HasMaxLength(50)
                    .HasColumnName("file_name");

                entity.Property(e => e.FileType)
                    .HasMaxLength(50)
                    .HasColumnName("file_type");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<QueueStatusPole>(entity =>
            {
                entity.ToTable("queue_status_poles", "queues");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.DateFile).HasColumnName("date_file");

                entity.Property(e => e.DateRegister).HasColumnName("date_register");

                entity.Property(e => e.Day).HasColumnName("day");

                entity.Property(e => e.FileName)
                    .HasMaxLength(50)
                    .HasColumnName("file_name");

                entity.Property(e => e.FileType)
                    .HasMaxLength(50)
                    .HasColumnName("file_type");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<QueueStatusLightning>(entity =>
            {
                entity.ToTable("queue_status_lightning", "queues");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.DateFile).HasColumnName("date_file");

                entity.Property(e => e.DateRegister).HasColumnName("date_register");

                entity.Property(e => e.Day).HasColumnName("day");

                entity.Property(e => e.FileName)
                    .HasMaxLength(50)
                    .HasColumnName("file_name");

                entity.Property(e => e.FileType)
                    .HasMaxLength(50)
                    .HasColumnName("file_type");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<QueueStatusCompensation>(entity =>
            {
                entity.ToTable("queue_status_compensation", "queues");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.DateFile).HasColumnName("date_file");

                entity.Property(e => e.DateRegister).HasColumnName("date_register");

                entity.Property(e => e.Day).HasColumnName("day");

                entity.Property(e => e.FileName)
                    .HasMaxLength(50)
                    .HasColumnName("file_name");

                entity.Property(e => e.FileType)
                    .HasMaxLength(50)
                    .HasColumnName("file_type");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<QueueStatusPoda>(entity =>
            {
                entity.ToTable("queue_status_podas", "queues");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.DateFile).HasColumnName("date_file");

                entity.Property(e => e.DateRegister).HasColumnName("date_register");

                entity.Property(e => e.Day).HasColumnName("day");

                entity.Property(e => e.FileName)
                    .HasMaxLength(50)
                    .HasColumnName("file_name");

                entity.Property(e => e.FileType)
                    .HasMaxLength(50)
                    .HasColumnName("file_type");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Year).HasColumnName("year");
            });

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
