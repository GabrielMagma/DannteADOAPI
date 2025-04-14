using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using ADO.BL.Interfaces;
using ADO.BL.Responses;
using Microsoft.Extensions.Configuration;

namespace ADO.BL.Services
{
    public class QueueGlobalServices : IQueueGlobalServices
    {
                
        private readonly IFileAssetValidationServices fileAssetValidationServices;
        private readonly IFileAssetProcessingServices fileAssetProcessingServices;
        private readonly ITT2FileValidationServices TT2ValidationServices;
        private readonly ITT2FileProcessingServices TT2ProcessingServices;
        private readonly ILacsFileValidationServices lacsValidationServices;
        private readonly ILacsFileProcessServices lacsProcessingServices;
        private readonly ISSPDFileValidationServices SSPDValidationServices;
        private readonly ISSPDFileProcessingServices SSPDProcessingServices;
        private readonly ITC1FileValidationServices TC1validationServices;
        private readonly ITC1FileProcessingServices TC1ProcessingServices;

        private readonly IStatusFileDataAccess statusFileDataAccess;

        private readonly string _AssetsDirectoryPath;
        private readonly string _AssetsDirectoryDestinyPath;
        private readonly string _tt2DirectoryPath;
        private readonly string _tt2DirectoryDestinyPath;
        private readonly string _FilesLACDirectoryPath;
        private readonly string _FilesLACDirectoryDestinyPath;
        private readonly string _FilesSSPDDirectoryPath;
        private readonly string _FilesSSPDDirectoryDestinyPath;
        private readonly string _Tc1DirectoryPath;
        private readonly string _Tc1DirectoryDestinyPath;

        public QueueGlobalServices(IConfiguration configuration,
            IFileAssetValidationServices _fileAssetValidationServices,
            IFileAssetProcessingServices _fileAssetProcessingServices,
            ITT2FileValidationServices _tt2ValidationServices,
            ITT2FileProcessingServices _tt2ProcessingServices,
            ILacsFileValidationServices _lacsValidationServices,
            ILacsFileProcessServices _lacsProcessingServices,
            ISSPDFileValidationServices _sspdValidationServices,
            ISSPDFileProcessingServices _sspdProcessingServices,
            ITC1FileValidationServices _tc1ValidationServices,
            ITC1FileProcessingServices _tc1ProcessingServices,
            IStatusFileDataAccess _statusFileDataAccess)
        {
            fileAssetValidationServices = _fileAssetValidationServices;
            fileAssetProcessingServices = _fileAssetProcessingServices;
            TT2ValidationServices = _tt2ValidationServices;
            TT2ProcessingServices = _tt2ProcessingServices;
            lacsValidationServices = _lacsValidationServices;
            lacsProcessingServices = _lacsProcessingServices;
            SSPDValidationServices = _sspdValidationServices;
            SSPDProcessingServices = _sspdProcessingServices;
            TC1validationServices = _tc1ValidationServices;
            TC1ProcessingServices = _tc1ProcessingServices;

            _AssetsDirectoryPath = configuration["FilesAssetsPath"];
            _AssetsDirectoryDestinyPath = configuration["FilesAssetsDestinyPath"];
            _tt2DirectoryPath = configuration["TT2DirectoryPath"];
            _tt2DirectoryDestinyPath = configuration["TT2DirectoryDestinyPath"];
            _FilesLACDirectoryPath = configuration["FilesLACPath"];
            _FilesLACDirectoryDestinyPath = configuration["FilesLACDestinyPath"];
            _FilesSSPDDirectoryPath = configuration["SspdDirectoryPath"];
            _FilesSSPDDirectoryDestinyPath = configuration["SspdDirectoryDestinyPath"];
            _Tc1DirectoryPath = configuration["Tc1DirectoryPath"];
            _Tc1DirectoryDestinyPath = configuration["Tc1DirectoryDestinyPath"];
            statusFileDataAccess = _statusFileDataAccess;
        }

        public async Task<ResponseQuery<string>> LaunchQueue(QueueValidationDTO request, ResponseQuery<string> response)
        {
            try
            {
                // assets

                //ResponseQuery<string> responseValidationAsset = new ResponseQuery<string>();
                //var requestValidationAsset = new FileAssetsValidationDTO();
                //requestValidationAsset.UserId = request.UserId;
                //responseValidationAsset = await fileAssetValidationServices.UploadFile(requestValidationAsset, responseValidationAsset);
                //if (!responseValidationAsset.Success)
                //{
                //    response.SuccessData = false;
                //    response.Message = $"Error in validation Asset, {responseValidationAsset.Message}";
                //    response.Success = false;
                //    return response;
                //}

                //ResponseQuery<string> responseProcessingAsset = new ResponseQuery<string>();
                //var requestProcessingAsset = new FileAssetsValidationDTO();
                //requestProcessingAsset.UserId = request.UserId;
                //responseProcessingAsset = await fileAssetProcessingServices.UploadFile(requestProcessingAsset, responseProcessingAsset);
                //if (!responseProcessingAsset.Success)
                //{
                //    response.SuccessData = false;
                //    response.Message = $"Error in Processing Asset, {responseProcessingAsset.Message}";
                //    response.Success = false;
                //    return response;
                //}

                // TT2

                //ResponseQuery<List<string>> responseValidationTt2 = new ResponseQuery<List<string>>();
                //var requestValidationTt2 = new TT2ValidationDTO();
                //requestValidationTt2.UserId = request.UserId;
                //responseValidationTt2 = await TT2ValidationServices.CompleteTT2Originals(requestValidationTt2, responseValidationTt2);
                //if (!responseValidationTt2.Success)
                //{
                //    response.SuccessData = false;
                //    response.Message = $"Error in validation TT2, {responseValidationTt2.Message}";
                //    response.Success = false;
                //    return response;
                //}

                //ResponseQuery<List<string>> responseProcessingTt2 = new ResponseQuery<List<string>>();
                //var requestProcessingTt2 = new TT2ValidationDTO();
                //requestProcessingTt2.UserId = request.UserId;
                //responseProcessingTt2 = await TT2ProcessingServices.CompleteTT2Originals(requestProcessingTt2, responseProcessingTt2);
                //if (!responseProcessingTt2.Success)
                //{
                //    response.SuccessData = false;
                //    response.Message = $"Error in Processing TT2, {responseProcessingTt2.Message}";
                //    response.Success = false;
                //    return response;
                //}

                // LAC

                //ResponseQuery<List<string>> responseValidationLac = new ResponseQuery<List<string>>();
                //var requestValidationLac = new LacValidationDTO();
                //requestValidationLac.UserId = request.UserId;
                //responseValidationLac = await lacsValidationServices.ReadFileLacOrginal(requestValidationLac, responseValidationLac);
                //if (!responseValidationLac.Success)
                //{
                //    response.SuccessData = false;
                //    response.Message = $"Error in validation Lacs, {responseValidationLac.Message}";
                //    response.Success = false;
                //    return response;
                //}

                //ResponseQuery<List<string>> responseProcessingLac = new ResponseQuery<List<string>>();
                //var requestProcessingLac = new LacValidationDTO();
                //requestProcessingLac.UserId = request.UserId;
                //responseProcessingLac = await lacsProcessingServices.ReadFileLacOrginal(requestProcessingLac, responseProcessingLac);
                //if (!responseProcessingLac.Success)
                //{
                //    response.SuccessData = false;
                //    response.Message = $"Error in Processing Lacs, {responseProcessingLac.Message}";
                //    response.Success = false;
                //    return response;
                //}

                // SSPD

                //ResponseQuery<List<string>> responseValidationSspd = new ResponseQuery<List<string>>();
                //var requestValidationSspd = new LacValidationDTO();
                //requestValidationSspd.UserId = request.UserId;
                //responseValidationSspd = await SSPDValidationServices.ReadFileSspdOrginal(requestValidationSspd, responseValidationSspd);
                //if (!responseValidationSspd.Success)
                //{
                //    response.SuccessData = false;
                //    response.Message = $"Error in validation Lacs, {responseValidationSspd.Message}";
                //    response.Success = false;
                //    return response;
                //}

                //ResponseQuery<List<string>> responseProcessingSspd = new ResponseQuery<List<string>>();
                //var requestProcessingSspd = new LacValidationDTO();
                //requestProcessingSspd.UserId = request.UserId;
                //responseProcessingSspd = await SSPDProcessingServices.ReadFileSspdOrginal(requestProcessingSspd, responseProcessingSspd);
                //if (!responseProcessingSspd.Success)
                //{
                //    response.SuccessData = false;
                //    response.Message = $"Error in validation Lacs, {responseProcessingSspd.Message}";
                //    response.Success = false;
                //    return response;
                //}

                // TC1

                //ResponseQuery<List<string>> responseValidationTc1 = new ResponseQuery<List<string>>();
                //var requestValidationTc1 = new TC1ValidationDTO();
                //requestValidationTc1.UserId = request.UserId;
                //responseValidationTc1 = await TC1validationServices.ReadAssets(requestValidationTc1, responseValidationTc1);
                //if (!responseValidationTc1.Success)
                //{
                //    response.SuccessData = false;
                //    response.Message = $"Error in validation Lacs, {responseValidationTc1.Message}";
                //    response.Success = false;
                //    return response;
                //}

                //ResponseQuery<List<string>> responseProcessingTc1 = new ResponseQuery<List<string>>();
                //var requestProcessingTc1 = new TC1ValidationDTO();
                //requestProcessingTc1.UserId = request.UserId;
                //responseProcessingTc1 = await TC1ProcessingServices.ReadAssets(requestProcessingTc1, responseProcessingTc1);
                //if (!responseProcessingTc1.Success)
                //{
                //    response.SuccessData = false;
                //    response.Message = $"Error in validation Lacs, {responseProcessingTc1.Message}";
                //    response.Success = false;
                //    return response;
                //}

                MoveFiles();


                response.SuccessData = true;
                response.Message = "Queue process completed";
                response.Success = true;
                return response;

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

        public void MoveFiles()
        {
            var listPaths = new List<string>()
            {
                _AssetsDirectoryPath,
                _tt2DirectoryPath,
                _FilesLACDirectoryPath,
                _FilesSSPDDirectoryPath,
                _Tc1DirectoryPath
            };

            var listEnds = new List<string>()
            {
                "_Correct",
                "_Error",
                "_unchanged",
                "_continues",
                "_continuesInvalid",
                "_closed",
                "_closedInvalid",
                "_continuesInsert",
                "_continuesUpdate",
                "_delete",
                "_update",
                "_insert",
                "_check",
                "_update"
            };

            var listStatusAssets = new List<QueueStatusAsset>();
            var listStatusTt2 = new List<QueueStatusTt2>();
            var listStatusLacs = new List<QueueStatusLac>();
            var listStatusSspd = new List<QueueStatusSspd>();
            var listStatusTc1 = new List<QueueStatusTc1>();

            foreach (var item in listPaths)
            {
                var destinationPath = $"{item}Destiny";
                var listFilesDestiny = new List<string>();
                foreach (var filePath in Directory.GetFiles(destinationPath, "*.csv"))
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    listFilesDestiny.Add($"{fileName}.csv");
                }
                foreach (var filePath in Directory.GetFiles(destinationPath, "*.xlsx"))
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    listFilesDestiny.Add($"{fileName}.xlsx");
                }


                foreach (var filePath in Directory.GetFiles(item, "*.csv"))
                {                    
                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    var nameTemp = fileName;

                    foreach (var item1 in listEnds)
                    {
                        nameTemp = nameTemp.Replace(item1, "");
                    }

                    if (nameTemp.Contains("_ASSET"))
                    {
                        var UnitStatusAssets = new QueueStatusAsset()
                        {
                            FileName = nameTemp,
                            Status = 5
                        };
                        var exist = listStatusAssets.FirstOrDefault(x => x.FileName == UnitStatusAssets.FileName);
                        if (exist == null)
                        {
                            listStatusAssets.Add(UnitStatusAssets);
                        }
                    }
                    else if (nameTemp.Contains("_TT2"))
                    {
                        var UnitStatusTt2 = new QueueStatusTt2()
                        {
                            FileName = nameTemp,
                            Status = 5
                        };
                        var exist = listStatusTt2.FirstOrDefault(x => x.FileName == UnitStatusTt2.FileName);
                        if (exist == null)
                        {
                            listStatusTt2.Add(UnitStatusTt2);
                        }
                    }
                    else if (nameTemp.Contains("_LAC"))
                    {
                        var UnitStatusLac = new QueueStatusLac()
                        {
                            FileName = nameTemp,
                            Status = 5
                        };
                        var exist = listStatusLacs.FirstOrDefault(x => x.FileName == UnitStatusLac.FileName);
                        if (exist == null)
                        {
                            listStatusLacs.Add(UnitStatusLac);
                        }
                    }
                    else if (nameTemp.Contains("_SSPD"))
                    {
                        var UnitStatusSspd = new QueueStatusSspd()
                        {
                            FileName = nameTemp,
                            Status = 5
                        };
                        var exist = listStatusSspd.FirstOrDefault(x => x.FileName == UnitStatusSspd.FileName);
                        if (exist == null)
                        {
                            listStatusSspd.Add(UnitStatusSspd);
                        }
                    }
                    else if (nameTemp.Contains("_TC1"))
                    {
                        var UnitStatusTc1 = new QueueStatusTc1()
                        {
                            FileName = nameTemp,
                            Status = 5
                        };
                        var exist = listStatusTc1.FirstOrDefault(x => x.FileName == UnitStatusTc1.FileName);
                        if (exist == null)
                        {
                            listStatusTc1.Add(UnitStatusTc1);
                        }
                    }                    

                    var fileDestination = Path.Combine(destinationPath, $"{fileName}.csv");
                    if (!Directory.Exists(destinationPath))
                        Directory.CreateDirectory(destinationPath);

                    if (!File.Exists(fileDestination))
                    {
                        File.Move(filePath, fileDestination);
                    }
                    else
                    {
                        File.Delete(filePath);
                    }
                }

                foreach (var filePath in Directory.GetFiles(item, "*.xlsx"))
                {                    
                    var fileName = Path.GetFileNameWithoutExtension(filePath);

                    var nameTemp = fileName;

                    foreach (var item1 in listEnds)
                    {
                        nameTemp = nameTemp.Replace(item1, "");
                    }

                    if (nameTemp.Contains("_ASSET"))
                    {
                        var UnitStatusAssets = new QueueStatusAsset()
                        {
                            FileName = nameTemp,
                            Status = 5
                        };
                        var exist = listStatusAssets.FirstOrDefault(x => x.FileName == UnitStatusAssets.FileName);
                        if (exist == null)
                        {
                            listStatusAssets.Add(UnitStatusAssets);
                        }
                    }
                    else if (nameTemp.Contains("_TT2"))
                    {
                        var UnitStatusTt2 = new QueueStatusTt2()
                        {
                            FileName = nameTemp,
                            Status = 5
                        };
                        var exist = listStatusTt2.FirstOrDefault(x => x.FileName == UnitStatusTt2.FileName);
                        if (exist == null)
                        {
                            listStatusTt2.Add(UnitStatusTt2);
                        }
                    }
                    else if (nameTemp.Contains("_LAC"))
                    {
                        var UnitStatusLac = new QueueStatusLac()
                        {
                            FileName = nameTemp,
                            Status = 5
                        };
                        var exist = listStatusLacs.FirstOrDefault(x => x.FileName == UnitStatusLac.FileName);
                        if (exist == null)
                        {
                            listStatusLacs.Add(UnitStatusLac);
                        }
                    }
                    else if (nameTemp.Contains("_SSPD"))
                    {
                        var UnitStatusSspd = new QueueStatusSspd()
                        {
                            FileName = nameTemp,
                            Status = 5
                        };
                        var exist = listStatusSspd.FirstOrDefault(x => x.FileName == UnitStatusSspd.FileName);
                        if (exist == null)
                        {
                            listStatusSspd.Add(UnitStatusSspd);
                        }
                    }
                    else if (nameTemp.Contains("_TC1"))
                    {
                        var UnitStatusTc1 = new QueueStatusTc1()
                        {
                            FileName = nameTemp,
                            Status = 5
                        };
                        var exist = listStatusTc1.FirstOrDefault(x => x.FileName == UnitStatusTc1.FileName);
                        if (exist == null)
                        {
                            listStatusTc1.Add(UnitStatusTc1);
                        }
                    }

                    

                    var fileDestination = Path.Combine(destinationPath, $"{fileName}.xlsx");
                    if (!Directory.Exists(destinationPath))
                        Directory.CreateDirectory(destinationPath);

                    if (!File.Exists(fileDestination))
                    {
                        File.Move(filePath, fileDestination);
                    }
                    else
                    {
                        File.Delete(filePath);
                    }

                }

                statusFileDataAccess.UpdateDataAssetList(listStatusAssets);
                statusFileDataAccess.UpdateDataTT2List(listStatusTt2);
                statusFileDataAccess.UpdateDataLACList(listStatusLacs);
                statusFileDataAccess.UpdateDataSSPDList(listStatusSspd);
                statusFileDataAccess.UpdateDataTC1List(listStatusTc1);
            }

        }                

    }
}
