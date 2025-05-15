using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO.BL.DTOs
{
    public partial class UpdateTrafoDTO
    {
        public string? code_sig { get; set; }
        public int? year { get; set; }
        public int? month { get; set; }
        public long? count { get; set; }
    }
}
