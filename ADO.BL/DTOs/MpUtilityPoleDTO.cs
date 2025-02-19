namespace ADO.BL.DTOs
{
    public class MpUtilityPoleDTO
    {
        public long Id { get; set; }
        public string? InventaryCode { get; set; }
        public string? PaintingCode { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Z { get; set; }
        public string? Fparent { get; set; }
        public long? IdRegion { get; set; }
        public string? NameRegion { get; set; }
        public int? TypePole { get; set; }
    }
}
