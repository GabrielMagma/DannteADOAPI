namespace ADO.BL.DTOs
{
    public class AssetsDTO
    {

        public string? Fparent { get; set; } = "-1";
        public string? Uia { get; set; } = "-1";
        public string? CodeSig { get; set; } = "-1";
        public long? IdZone { get; set; } = -1;
        public string? NameZone { get; set; } = "NO DATA";
        public long? IdRegion { get; set; } = -1;
        public string? NameRegion { get; set; } = "NO DATA";
        public long? IdLocality { get; set; } = -1;
        public string? NameLocality { get; set; } = "NO DATA";
        public long? IdSector { get; set; } = -1;
        public string? NameSector { get; set; } = "NO DATA";
        public long? GeographicalCode { get; set; } = -1;
        public float? Latitude { get; set; } = 0;
        public float? Longitude { get; set; } = 0;

    }
}