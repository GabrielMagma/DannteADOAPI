using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class MpUtilityPole
    {
        public long Id { get; set; }
        public string InventaryCode { get; set; } = null!;
        public string PaintingCode { get; set; } = null!;
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Z { get; set; }
        public string Fparent { get; set; } = null!;
        public long IdRegion { get; set; }
        public string NameRegion { get; set; } = null!;
    }
}
