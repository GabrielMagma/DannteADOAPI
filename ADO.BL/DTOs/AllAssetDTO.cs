﻿namespace ADO.BL.DTOs
{
    public class AllAssetDTO
    {
        public long Id { get; set; }
        public string? TypeAsset { get; set; } = "-1";
        public string? CodeSig { get; set; } = "-1";
        public string? Uia { get; set; } = "-1";
        public string? Codetaxo { get; set; } = "-1";
        public string? Fparent { get; set; } = "-1";
        public float? Latitude { get; set; } = 0;
        public float? Longitude { get; set; } = 0;
        public string? Poblation { get; set; } = "-1";
        public string? Group015 { get; set; } = "-1";
        public string? Uccap14 { get; set; } = "-1";
        public DateOnly? DateInst { get; set; }
        public DateOnly? DateUnin { get; set; }
        public int? State { get; set; } = 2;        
        public long? IdRegion { get; set; } = -1;
        public string? NameRegion { get; set; } = "NO DATA";        
        public string? Address { get; set; } = "-1";
        public int? Year { get; set; }
        public int? Month { get; set; }
        public long? IdZone { get; set; }
        public string? NameZone { get; set; }
        public long? IdLocality { get; set; }
        public string? NameLocality { get; set; }
        public long? IdSector { get; set; }
        public string? NameSector { get; set; }
        public long? GeographicalCode { get; set; }
    }

}
