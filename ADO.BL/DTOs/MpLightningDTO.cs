namespace ADO.BL.DTOs
{
    public class MpLightningDTO
    {
        public long Id { get; set; }
        public string? NameRegion { get; set; }
        public string? NameZone { get; set; }
        public string? NameLocality { get; set; }
        public string? Fparent { get; set; }
        public DateTime? DateEvent { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float? Amperage { get; set; }
        public float? Error { get; set; }
        public int? Type { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
    }
}
