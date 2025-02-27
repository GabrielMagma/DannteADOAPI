using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class Poda
    {
        public long Id { get; set; }
        public string? NameRegion { get; set; }
        public string? NameZone { get; set; }
        public string? NameLocation { get; set; }
        public DateOnly? DateExecuted { get; set; }
        public string? Scheduled { get; set; }
        public DateOnly? DateState { get; set; }
        public string? Pqr { get; set; }
        public string? NoReport { get; set; }
        public string? Consig { get; set; }
        public string? BeginSup { get; set; }
        public string? EndSup { get; set; }
        public string? Urban { get; set; }
        public string? Description { get; set; }
        public string? NoOt { get; set; }
        public string? Circuit { get; set; }
        public string? StateOt { get; set; }
        public string? Item { get; set; }
    }
}
