namespace ADO.BL.DTOs
{
    public class StatusFileDTO
    {
        public long Id { get; set; }
        public string? FileType { get; set; }
        public string? FileName { get; set; }
        public long? UserId { get; set; }
        public DateOnly? DateFile { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public int? Status { get; set; }

        // fileType = TC1, TT2, Lac, sspd, tt9...
        // status = 0 => file error
        // status = 1 => file loaded
        // status = 2 => file processed

    }

}
