using System;
using System.Collections.Generic;

namespace ADO.BL.DataEntities
{
    public partial class FilesTt2
    {
        public long Id { get; set; }
        public string Uia { get; set; } = null!;
        public string CodeSig { get; set; } = null!;
        public int State { get; set; }
        public DateOnly StateDate { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public string? Files { get; set; }
        public DateOnly? FilesDate { get; set; }
    }
}
