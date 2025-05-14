using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class MpTransformerBurned
    {
        public long Id { get; set; }
        public string CodeSig { get; set; } = null!;
        public int Year { get; set; }
        public int Month { get; set; }
        public int Total { get; set; }
        public string Fparent { get; set; } = null!;
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public DateTime FailureDate { get; set; }
        public DateTime RetireDate { get; set; }
        public DateTime ChangeDate { get; set; }
    }
}
