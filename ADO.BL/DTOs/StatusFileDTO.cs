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
        public DateOnly? DateRegister { get; set; }

        // fileType = TC1, TT2, LAC, SSPD, IO, ASSET
        // status = 0 => sin cargar
        // status = 1 => archivo cargado
        // status = 2 => validación fallida
        // status = 3 => procesamiento fallido
        // status = 4 => procesamiento exitoso
        // status = 5 => finalizado

    }

}
