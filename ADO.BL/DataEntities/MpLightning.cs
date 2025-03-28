﻿using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class MpLightning
    {
        public long Id { get; set; }
        public string NameRegion { get; set; } = null!;
        public string NameZone { get; set; } = null!;
        public string? NameLocality { get; set; }
        public string Fparent { get; set; } = null!;
        public DateTime DateEvent { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float? Amperage { get; set; }
        public float? Error { get; set; }
        public int? Type { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
