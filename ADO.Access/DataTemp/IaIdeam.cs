using System;
using System.Collections.Generic;

namespace ADO.Access.DataTemp
{
    public partial class IaIdeam
    {
        public int Id { get; set; }
        public string? Stationcode { get; set; }
        public string Stationname { get; set; } = null!;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Altitude { get; set; }
        public string? Department { get; set; }
        public string? Municipality { get; set; }
        public string? Parameterid { get; set; }
        public string? Frequency { get; set; }
        public DateOnly? Date { get; set; }
        public double? Precipitation { get; set; }
    }
}
