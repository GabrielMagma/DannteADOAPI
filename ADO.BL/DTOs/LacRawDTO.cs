using System;
using System.Collections.Generic;

namespace ADO.BL.DTOs
{
    public partial class LacRawDTO
    {
        public long Id { get; set; }
        public string? EventCode { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Uia { get; set; }
        public string? ElementType { get; set; }
        public string? EventCause { get; set; }
        public string? EventContinues { get; set; }
        public string? EventExcluidZin { get; set; }
        public string? AffectsConnection { get; set; }
        public string? LightingUsers { get; set; }
        
    }
}
