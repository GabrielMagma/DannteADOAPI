using ADO.BL.DTOs;
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
        public Task<ResponseQuery<List<string>>> ReadAssets(TC1ValidationDTO request, ResponseQuery<List<string>> response);
    }
}
