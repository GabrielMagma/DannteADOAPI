using System;
using System.Collections.Generic;

namespace ADO.BL.DTOs
{
    public partial class RamalesColumnsDTO
    {        
        public int CodEvento { get; set; }
        public int FechaIni { get; set; }
        public int FechaFin { get; set; }
        public int Duracion { get; set; }
        public int Fparent { get; set; }
        public int CodInter { get; set; }
        public int NombreInter { get; set; }
        public int ApoyoApertura { get; set; }
        public int ApoyoFalla { get; set; }
        public int CodCausaEvent { get; set; }
        public int TotalTrafo { get; set; }
        public int TotalCliente { get; set; }
        public int TotalOpe { get; set; }
    }
}
