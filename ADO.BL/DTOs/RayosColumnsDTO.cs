using System;
using System.Collections.Generic;

namespace ADO.BL.DTOs
{
    public partial class RayosColumnsDTO
    {        
        public int Fecha { get; set; }
        public int Region { get; set; }
        public int Zona { get; set; }
        public int Circuito { get; set; }
        public int Latitud { get; set; }
        public int Longitud { get; set; }
        public int Tipo { get; set; }
        public int Corriente { get; set; }
        public int Error { get; set; }
        public int Municipio { get; set; }        
    }
}
