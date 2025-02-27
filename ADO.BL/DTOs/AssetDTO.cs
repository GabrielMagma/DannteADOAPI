namespace ADO.BL.DTOs
{
    public class AssetDTO
    {
        public string? Uia { get; set; }
        public string? Code_sig { get; set; }
        public DateOnly? DateInst { get; set; }        
        public int? State { get; set; } = 2;
    }
}
