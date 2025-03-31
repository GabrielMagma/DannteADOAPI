using System;
using System.Collections.Generic;

namespace ADO.BL.DTOs
{
    public partial class IOsValidationDTO
    {
        public long UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public string? NombreArchivo { get; set; }
        public string? Empresa { get; set; }
        public string? Ruta { get; set; }
        public bool? Encabezado { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
    }
}
