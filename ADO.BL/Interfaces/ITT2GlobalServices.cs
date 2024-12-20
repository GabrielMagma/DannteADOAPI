using ADO.BL.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO.BL.Interfaces
{
    public interface ITT2GlobalServices
    {
        public Task<ResponseQuery<List<string>>> CompleteTT2Originals(ResponseQuery<List<string>> response);
    }
}
