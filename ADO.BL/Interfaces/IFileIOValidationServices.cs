﻿using ADO.BL.DTOs;
using ADO.BL.Responses;
using Microsoft.AspNetCore.Http;

namespace ADO.BL.Interfaces
{
    public interface IFileIOValidationServices
    {
        public Task<ResponseQuery<string>> UploadIO(IOsValidationDTO iosValidation, ResponseQuery<string> response);
    }
}
