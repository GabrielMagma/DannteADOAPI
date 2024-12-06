using ADO.BL.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO.BL.Interfaces
{
    public interface ITC1Services
    {
        public ResponseQuery<List<string>> ReadAssets(ResponseQuery<List<string>> response);
    }
}
