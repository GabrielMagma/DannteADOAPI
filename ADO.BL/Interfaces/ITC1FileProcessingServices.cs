using ADO.BL.DTOs;
using ADO.BL.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO.BL.Interfaces
{
    public interface ITC1FileProcessingServices
    {
        public Task<ResponseQuery<bool>> ReadFilesTc1(TC1ValidationDTO request, ResponseQuery<bool> response);
    }
}
