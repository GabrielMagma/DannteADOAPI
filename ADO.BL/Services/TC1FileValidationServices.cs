﻿using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OfficeOpenXml.Drawing.Style.Fill;
using System.Globalization;
using System.Text;

namespace ADO.BL.Services
{
    public class TC1FileValidationServices : ITC1FileValidationServices
    {
        private string _connectionString;
        private readonly string _connectionStringEssa;
        private readonly string _connectionStringEep;
        private readonly string _assetsDirectoryPath;
        private readonly string[] _timeFormats;
        private readonly ITC1ValidationServices _ITC1ValidationServices;        
        private readonly IStatusFileDataAccess statusFileDataAccess;
        private readonly IMapper mapper;

        public TC1FileValidationServices(IConfiguration configuration, 
            ITC1ValidationServices Itc1ValidationServices,            
            IStatusFileDataAccess _statusFileDataAccess,
            IMapper _mapper)
        {
            _connectionStringEssa = configuration.GetConnectionString("PgDbTestingConnection");
            _connectionStringEep = configuration.GetConnectionString("PgDbEepConnection");
            _assetsDirectoryPath = configuration["Tc1DirectoryPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _ITC1ValidationServices = Itc1ValidationServices;            
            statusFileDataAccess = _statusFileDataAccess;
            mapper = _mapper;
            
        }

        public async Task<ResponseQuery<List<string>>> ReadAssets(TC1ValidationDTO request, ResponseQuery<List<string>> response)
        {
            try
            {
                _connectionString = request.Empresa == "ESSA" ? _connectionStringEssa : _connectionStringEep;
                var responseError = new ResponseEntity<List<StatusFileDTO>>();
                var ErrorinFiles = await _ITC1ValidationServices.ValidationTC1(request, responseError);
                if (ErrorinFiles.Success == false)
                {
                    response.Message = "Archivo con errores";
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {
                    //update status queue
                    var subgroupMap = mapper.Map<List<QueueStatusTc1>>(ErrorinFiles.Data);                    
                    var resultSave = await statusFileDataAccess.UpdateDataTC1List(subgroupMap);                    

                    response.Message = "Proceso completado para todos los archivos";
                    response.SuccessData = true;
                    response.Success = true;
                    return response;
                }
            }
            catch (FormatException ex)
            {
                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Success = false;
                response.SuccessData = false;
            }

            return response;
        }

    }
}
