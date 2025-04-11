namespace ADO.BL.DTOs
{
    public class MpGeolocalityDTO
    {
        public long? id { get; set; }
        public string? code_sig { get; set; }        
        public string? codigo_geografico { get; set; }
        public string? nombre_categoria { get; set; }
        public string? zona { get; set; }
        public string? region { get; set; }
        public string? localidad { get; set; }
        public string? municipio { get; set; }
        public string? grupo_calidad { get; set; }
    }
}
