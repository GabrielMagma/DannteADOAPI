using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class QueueStatusTransformerBurned
    {
        public long Id { get; set; }
        public string? FileType { get; set; }
        public string? FileName { get; set; }
        public long? UserId { get; set; }
        public DateOnly? DateFile { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public int? Status { get; set; }
        public DateOnly? DateRegister { get; set; }
    }
}
