namespace ADO.BL.DTOs
{
    public class FilesTc1DTO
    {
        public long Id { get; set; }

        public string Niu { get; set; } = null!;

        public string Uia { get; set; } = null!;

        public int Year { get; set; }

        public int Month { get; set; }

        public string? Files { get; set; }

        public DateOnly? FilesDate { get; set; }
    }
}
