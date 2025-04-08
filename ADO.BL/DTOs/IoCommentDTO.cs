namespace ADO.BL.DTOs
{
    public class IoCommentDTO
    {
        public long Id { get; set; }
        public string? FileName { get; set; }
        public int? FileLine { get; set; }
        public string? Comment { get; set; }
        public string? AffectedSector { get; set; }
    }

}
