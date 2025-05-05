namespace ADO.BL.DataEntities
{
    public partial class MpUtilityPole
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
