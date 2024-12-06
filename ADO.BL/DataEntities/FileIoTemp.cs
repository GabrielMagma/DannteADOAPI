using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class FileIoTemp
    {
        public long Id { get; set; }
        public string CodigoEvento { get; set; } = null!;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFinal { get; set; }
        public float Duracion { get; set; }
        public string CodigoCircuito { get; set; } = null!;
        public string CodInterruptor { get; set; } = null!;
        public string NombreTipoInterruptor { get; set; } = null!;
        public string ApoyoApertura { get; set; } = null!;
        public string ApoyoFalla { get; set; } = null!;
        public int CodigoCausaEvento { get; set; }
        public int TotalTrafo { get; set; }
        public int TotalClientes { get; set; }
        public int TotalOperaciones { get; set; }
    }
}
