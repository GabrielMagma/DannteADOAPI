using System;
using System.Collections.Generic;

namespace ADO.BL.DTOs
{
    public partial class LacColumnsDTO
    {        
        public int EventCode { get; set; }
        public int StartDate { get; set; }
        public int EndDate { get; set; }
        public int Uia { get; set; }        
        public int EventContinues { get; set; }                
    }
}
