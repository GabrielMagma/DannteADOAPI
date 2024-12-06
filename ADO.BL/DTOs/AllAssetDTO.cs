namespace ADO.BL.DTOs
{
    public class AllAssetDTO
    {
        public long Id { get; set; }
        public string? TypeAsset { get; set; } = "-1";
        public string? CodeSig { get; set; } = "-1";
        public string? Uia { get; set; } = "-1";
        public string? Codetaxo { get; set; } = "-1";
        public string? Fparent { get; set; } = "-1";
        public float? Latitude { get; set; } = 0;
        public float? Longitude { get; set; } = 0;
        public string? Poblation { get; set; } = "-1";
        public string? Group015 { get; set; } = "-1";
        public DateOnly? DateInst { get; set; }
        public DateOnly? DateUnin { get; set; }
        public int? State { get; set; } = 2;
        public string? Uccap14 { get; set; } = "-1";
        public long? IdZone { get; set; } = -1;
        public string? NameZone { get; set; } = "NO DATA";
        public long? IdRegion { get; set; } = -1;
        public string? NameRegion { get; set; } = "NO DATA";
        public long? IdLocality { get; set; } = -1;
        public string? NameLocality { get; set; } = "NO DATA";
        public long? IdSector { get; set; } = -1;
        public string? NameSector { get; set; } = "NO DATA";
        public long? GeographicalCode { get; set; } = -1;

        public string? Address { get; set; } = "-1";
    }

}
