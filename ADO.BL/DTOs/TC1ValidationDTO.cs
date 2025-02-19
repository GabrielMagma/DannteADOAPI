using System;
using System.Collections.Generic;

namespace ADO.BL.DTOs
{
    public partial class TC1ValidationDTO
    {
        public long UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public string? NombreArchivo { get; set; }
        public string? Ruta { get; set; }
        public bool? Encabezado { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public Tc1ColumnsDTO? columns { get; set; }                
    }
}
