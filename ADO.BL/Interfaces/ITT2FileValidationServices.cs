using ADO.BL.DTOs;
using ADO.BL.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO.BL.Interfaces
{
    public interface ITT2FileValidationServices
    {
        public Task<ResponseQuery<bool>> ReadFilesTT2(TT2ValidationDTO request, ResponseQuery<bool> response);
    }
}
