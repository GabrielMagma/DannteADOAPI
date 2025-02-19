using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class FilesTc1
    {
        public long Id { get; set; }
        public string Niu { get; set; } = null!;
        public string Uia { get; set; } = null!;
        public int? Year { get; set; }
        public int? Month { get; set; }
        public string? Files { get; set; }
        public DateOnly? FilesDate { get; set; }
    }
}
