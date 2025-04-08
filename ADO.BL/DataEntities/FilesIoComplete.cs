using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class FilesIoComplete
    {
        public long Id { get; set; }
        public DateOnly? DateIo { get; set; }
        public string? CodeGis { get; set; }
        public string? Location { get; set; }
        public string? Ubication { get; set; }
        public string? Element { get; set; }
        public string? Component { get; set; }
        public DateTime? HourOut { get; set; }
        public DateTime? HourIn { get; set; }
        public float? MinInterruption { get; set; }
        public float? HourInterruption { get; set; }
        public string? DescCause { get; set; }
        public int? CodCauseEvent { get; set; }
        public int? Cause { get; set; }
        public string? Maneuver { get; set; }
        public string? FuseQuant { get; set; }
        public string? FuseCap { get; set; }
        public int? CodeConsig { get; set; }
        public string? TypeEvent { get; set; }
        public string? Dependency { get; set; }
        public float? OutPower { get; set; }
        public float? DnaKwh { get; set; }
        public int? Users { get; set; }
        public string? ApplicationId { get; set; }
        public float? CapacityKva { get; set; }
        public string? Type { get; set; }
        public string? Ownership { get; set; }
        public long? AffectedSector { get; set; }
        public long? Observation { get; set; }
    }
}
