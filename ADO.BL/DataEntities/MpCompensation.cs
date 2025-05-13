using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class MpCompensation
    {
        public int Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Fparent { get; set; } = null!;
        public string CodeSig { get; set; } = null!;
        public string? QualityGroup { get; set; }
        public string? TensionLevel { get; set; }
        public string Nui { get; set; } = null!;
        public float Vcf { get; set; }
        public float Vcd { get; set; }
        public float Vc { get; set; }
        public float? Longitude { get; set; }
        public float? Latitude { get; set; }
    }
}
