using ADO.BL.DataEntities;
using Microsoft.EntityFrameworkCore;

namespace ADO.Access.DataEep
{
    public partial class DannteEepTestingContext : DbContext
    {
        public DannteEepTestingContext()
        {
        }

        public DannteEepTestingContext(DbContextOptions<DannteEepTestingContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AllAsset> AllAssets { get; set; } = null!;
        public virtual DbSet<AllAssetNew> AllAssetNews { get; set; } = null!;
        public virtual DbSet<Ideam> Ideams { get; set; } = null!;
        public virtual DbSet<StatusFile> StatusFiles { get; set; } = null!;
        public virtual DbSet<FilesIo> FilesIos { get; set; } = null!;
        public virtual DbSet<MpUtilityPole> MpUtilityPoles { get; set; } = null!;
        public virtual DbSet<FilesIoComplete> FilesIoCompletes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AllAsset>(entity =>
            {
                entity.ToTable("all_asset");

                entity.HasIndex(e => new { e.Uia, e.IdRegion, e.IdLocality }, "idx_all_asset");

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

            modelBuilder.Entity<AllAssetNew>(entity =>
            {
                entity.ToTable("all_asset_new", "machine");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

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

            modelBuilder.Entity<Ideam>(entity =>
            {
                entity.ToTable("ideam", "machine");

                entity.Property(e => e.Id).HasColumnName("id");

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

            modelBuilder.Entity<StatusFile>(entity =>
            {
                entity.ToTable("StatusFile", "machine");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.FileName).HasMaxLength(20);

                entity.Property(e => e.FileType).HasMaxLength(10);
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

            modelBuilder.Entity<MpUtilityPole>(entity =>
            {
                entity.ToTable("mp_utility_pole", "maps");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Fparent)
                    .HasMaxLength(20)
                    .HasColumnName("fparent");

                entity.Property(e => e.IdRegion).HasColumnName("id_region");

                entity.Property(e => e.InventaryCode)
                    .HasMaxLength(50)
                    .HasColumnName("inventary_code");

                entity.Property(e => e.Latitude).HasColumnName("latitude");

                entity.Property(e => e.Longitude).HasColumnName("longitude");

                entity.Property(e => e.NameRegion)
                    .HasMaxLength(50)
                    .HasColumnName("name_region");

                entity.Property(e => e.PaintingCode)
                    .HasMaxLength(50)
                    .HasColumnName("painting_code");

                entity.Property(e => e.TypePole).HasColumnName("type_pole");

                entity.Property(e => e.X).HasColumnName("x");

                entity.Property(e => e.Y).HasColumnName("y");

                entity.Property(e => e.Z).HasColumnName("z");
            });

            modelBuilder.Entity<FilesIoComplete>(entity =>
            {
                entity.ToTable("files_io_complete");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.AffectedSector).HasMaxLength(50);

                entity.Property(e => e.ApplicationId).HasColumnType("character varying");

                entity.Property(e => e.CapacityKva).HasColumnName("CapacityKVA");

                entity.Property(e => e.CodeGis).HasMaxLength(50);

                entity.Property(e => e.Component).HasMaxLength(50);

                entity.Property(e => e.Dependency).HasColumnType("character varying");

                entity.Property(e => e.DescCause).HasMaxLength(50);

                entity.Property(e => e.Element).HasMaxLength(50);

                entity.Property(e => e.FuseCap).HasColumnType("character varying");

                entity.Property(e => e.FuseQuant).HasColumnType("character varying");

                entity.Property(e => e.HourIn).HasColumnType("timestamp without time zone");

                entity.Property(e => e.HourOut).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Location).HasMaxLength(50);

                entity.Property(e => e.Maneuver).HasMaxLength(50);

                entity.Property(e => e.Observation).HasMaxLength(50);

                entity.Property(e => e.Ownership).HasMaxLength(50);

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.TypeEvent).HasColumnType("character varying");

                entity.Property(e => e.Ubication).HasMaxLength(50);
            });

            modelBuilder.HasSequence("causalsimulationtypes_id_seq", "simulate").HasMax(2147483647);

            modelBuilder.HasSequence("simulation_code_sim_seq", "simulate").HasMax(2147483647);

            modelBuilder.HasSequence("simulation_history_id_seq", "simulate").HasMax(2147483647);

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
