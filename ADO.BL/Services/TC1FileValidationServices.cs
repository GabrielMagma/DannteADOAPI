using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

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
        private readonly IHubContext<NotificationHub> _hubContext;
        public TC1FileValidationServices(IConfiguration configuration, 
            ITC1ValidationServices Itc1ValidationServices,            
            IStatusFileDataAccess _statusFileDataAccess,
            IMapper _mapper,
            IHubContext<NotificationHub> hubContext)
        {
            _connectionStringEssa = configuration.GetConnectionString("PgDbTestingConnection");
            _connectionStringEep = configuration.GetConnectionString("PgDbEepConnection");
            _assetsDirectoryPath = configuration["Tc1DirectoryPath"];
            _timeFormats = configuration.GetSection("DateTimeFormats").Get<string[]>();
            _ITC1ValidationServices = Itc1ValidationServices;            
            statusFileDataAccess = _statusFileDataAccess;
            mapper = _mapper;
            _hubContext = hubContext;
        }

        public async Task<ResponseQuery<bool>> ReadFilesTc1(TC1ValidationDTO request, ResponseQuery<bool> response)
        {
            try
            {
                _connectionString = request.Empresa == "ESSA" ? _connectionStringEssa : _connectionStringEep;
                var responseError = new ResponseEntity<List<StatusFileDTO>>();
                var ErrorinFiles = await _ITC1ValidationServices.ValidationTC1(request, responseError);
                if (ErrorinFiles.Success == false)
                {
                    response.Message = ErrorinFiles.Message;
                    response.SuccessData = false;
                    response.Success = false;
                    return response;
                }
                else
                {
                    //update status queue
                    var subgroupMap = mapper.Map<List<QueueStatusTc1>>(ErrorinFiles.Data);                    
                    var resultSave = await statusFileDataAccess.UpdateDataTC1List(subgroupMap);                    

                    response.Message = "All files created";
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
