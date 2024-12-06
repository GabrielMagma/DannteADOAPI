using System;
using System.Collections.Generic;

namespace ADO.BL.DTOs
{
    public partial class FilesSspdDTO
    {
        public string EventCode { get; set; } = null;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Uia { get; set; } = "NO DATA";
        public int? ElementType { get; set; } = -1;
        public int? EventCause { get; set; } = -1;
        public string? EventContinues { get; set; } 
        public int? EventExcluidZin { get; set; } = -1;
        public int? AffectsConnection { get; set; } = -1;
        public int? LightingUsers { get; set; } = -1;
        public int? Year { get; set; } = -1;
        public int? Month { get; set; } = -1;

        public int? State { get; set; } = -1;

        public string? FileCode { get; set; } = "NO DATA";

    }
}
