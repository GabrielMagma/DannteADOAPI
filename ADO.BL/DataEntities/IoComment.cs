using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class IoComment
    {
        public long Id { get; set; }
        public string? FileName { get; set; }
        public int? FileLine { get; set; }
        public string? Comment { get; set; }
        public string? AffectedSector { get; set; }
    }
}
