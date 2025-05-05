namespace ADO.BL.DTOs
{
    public class MpUtilityPoleDTO
    {
        public long Id { get; set; }
        public string InventaryCode { get; set; } = null!;
        public string PaintingCode { get; set; } = null!;
        public float Latitude { get; set; }
        public float Longitude { get; set; }        
        public string Fparent { get; set; } = null!;
        public int? TypePole { get; set; }
    }
}
