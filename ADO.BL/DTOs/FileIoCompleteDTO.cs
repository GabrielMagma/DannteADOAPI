namespace ADO.BL.DTOs
{
    public class FileIoCompleteDTO
    {
        public long Id { get; set; }
        public DateOnly DateIo { get; set; }
        public string CodeGis { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Ubication { get; set; } = null!;
        public string Element { get; set; } = null!;
        public string Component { get; set; } = null!;        
        public DateTime HourOut { get; set; }
        public DateTime HourIn { get; set; }
        public float MinInterruption { get; set; }
        public float HourInterruption { get; set; }
        public string DescCause { get; set; } = null!;
        public int CodCauseEvent { get; set; }
        public int Cause { get; set; }        
        public string Maneuver { get; set; } = null!;
        public string FuseQuant { get; set; } = null!;
        public string FuseCap { get; set; } = null!;
        public int CodeConsig { get; set; }
        public string TypeEvent { get; set; } = null!;
        public string Dependency { get; set; } = null!;
        public float OutPower { get; set; }
        public float DnaKwh { get; set; }
        public int Users { get; set; }
        public string ApplicationId { get; set; } = null!;
        public float CapacityKva { get; set; }
        public string Type { get; set; } = null!;
        public string Ownership { get; set; } = null!;
        public long? AffectedSector { get; set; }
        public long? Observation { get; set; }
    }
}
