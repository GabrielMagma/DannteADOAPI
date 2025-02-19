using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class FilesLac
    {
        public long Id { get; set; }
        public string EventCode { get; set; } = null!;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Uia { get; set; } = null!;
        public string? Fparent { get; set; }
        public int ElementType { get; set; }
        public int EventCause { get; set; }
        public string EventContinues { get; set; } = null!;
        public int EventExcluidZin { get; set; }
        public int AffectsConnection { get; set; }
        public int LightingUsers { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public string? Files { get; set; }
        public DateOnly? FilesDate { get; set; }
        public int? State { get; set; }
        public string? FileCode { get; set; }
    }
}
