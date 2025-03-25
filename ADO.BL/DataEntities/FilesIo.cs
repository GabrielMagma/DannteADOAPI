namespace ADO.BL.DataEntities
{
    public partial class FilesIo
    {
        public long IdTb { get; set; }
        public DateOnly DateIo { get; set; }
        public string CodeSig { get; set; } = null!;
        public string TypeAsset { get; set; } = null!;
        public string Fparent { get; set; } = null!;
        public string Element { get; set; } = null!;
        public string Component { get; set; } = null!;
        public DateTime HourOut { get; set; }
        public DateTime HourIn { get; set; }
        public float MinInterruption { get; set; }
        public float HourInterruption { get; set; }
        public int CregCause { get; set; }
        public int Cause { get; set; }
        public string EventType { get; set; } = null!;
        public string Dependence { get; set; } = null!;
        public int Users { get; set; }
        public float DnaKwh { get; set; }
        public int Failure { get; set; }
        public string Maneuver { get; set; } = null!;
        public string FileIo { get; set; } = null!;
        public int Year { get; set; }
        public int Month { get; set; }
        public DateOnly FilesDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}
