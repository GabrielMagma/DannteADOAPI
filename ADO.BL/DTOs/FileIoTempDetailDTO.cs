namespace ADO.BL.DTOs
{
    public class FileIoTempDetailDTO
    {
        public long Id { get; set; }
        public string CodigoEvento { get; set; } = null!;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFinal { get; set; }
        public float Duracion { get; set; }
        public string CodigoCircuito { get; set; } = null!;
        public string CodInteruptor { get; set; } = null!;
        public string NombreTipoInteruptor { get; set; } = null!;
        public string ApoyoApertura { get; set; } = null!;
        public string ApoyoFalla { get; set; } = null!;
        public int CodigoCausaEvento { get; set; }
        public int TotalTafo { get; set; }
        public int TotalClientes { get; set; }
        public int TotalOperaciones { get; set; }
        public string UiaTrafo { get; set; } = null!;
    }
}
