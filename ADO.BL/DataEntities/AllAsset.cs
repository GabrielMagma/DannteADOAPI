using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class AllAsset
    {
        public long Id { get; set; }
        public string? TypeAsset { get; set; }
        public string? CodeSig { get; set; }
        public string? Uia { get; set; }
        public string? Codetaxo { get; set; }
        public string? Fparent { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public string? Poblation { get; set; }
        public string? Group015 { get; set; }
        public string? Uccap14 { get; set; }
        public DateOnly? DateInst { get; set; }
        public DateOnly? DateUnin { get; set; }
        public int? State { get; set; }        
        public long? IdRegion { get; set; }
        public string? NameRegion { get; set; }        
        public string? Address { get; set; }        
        public int? Year { get; set; }
        public int? Month { get; set; }
    }
}
