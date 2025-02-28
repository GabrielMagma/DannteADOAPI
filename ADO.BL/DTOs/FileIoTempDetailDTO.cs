namespace ADO.BL.DTOs
{
    public class FileIoTempDetailDTO
    {
        public long Id { get; set; }
        public string CodeEvent { get; set; } = null!;
        public DateOnly BeginDate { get; set; }
        public DateOnly EndDate { get; set; }
        public float Duration { get; set; }
        public string Fparent { get; set; } = null!;
        public string CodeSwitch { get; set; } = null!;
        public string NameTypeSwitch { get; set; } = null!;
        public string SupportOpen { get; set; } = null!;
        public string SupportFailure { get; set; } = null!;
        public int CodeCauseEvent { get; set; }
        public int TotalTrafo { get; set; }
        public int TotalClients { get; set; }
        public int TotalOperations { get; set; }
        public string UiaTrafo { get; set; } = null!;
    }
}
