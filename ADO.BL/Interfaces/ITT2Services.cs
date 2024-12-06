using ADO.BL.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO.BL.Interfaces
{
    public interface ITT2Services
    {
        public ResponseQuery<List<string>> CompleteTT2Originals(ResponseQuery<List<string>> response);

        public ResponseQuery<List<string>> CreateTT2Files(ResponseQuery<List<string>> response);

        public ResponseQuery<List<string>> UpdateAllAssetByTT2(ResponseQuery<List<string>> response);
    }
}
