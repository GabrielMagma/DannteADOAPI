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
            ITC1FileProcessingServices _tc1ProcessingServices)
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
        }

        public async Task<ResponseQuery<string>> LaunchQueue(QueueValidationDTO request, ResponseQuery<string> response)
        {
            try
            {
                // assets
                ResponseQuery<string> responseValidationAsset = new ResponseQuery<string>();
                var requestValidationAsset = new FileAssetsValidationDTO();
                requestValidationAsset.UserId = request.UserId;
                responseValidationAsset = await fileAssetValidationServices.UploadFile(requestValidationAsset, responseValidationAsset);
                if (!responseValidationAsset.Success)
                {
                    response.SuccessData = false;
                    response.Message = $"Error in validation Asset, {responseValidationAsset.Message}";
                    response.Success = false;
                    return response;
                }

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

                ResponseQuery<List<string>> responseValidationTt2 = new ResponseQuery<List<string>>();
                var requestValidationTt2 = new TT2ValidationDTO();
                requestValidationTt2.UserId = request.UserId;
                responseValidationTt2 = await TT2ValidationServices.CompleteTT2Originals(requestValidationTt2, responseValidationTt2);
                if (!responseValidationTt2.Success)
                {
                    response.SuccessData = false;
                    response.Message = $"Error in validation TT2, {responseValidationTt2.Message}";
                    response.Success = false;
                    return response;
                }

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

                ResponseQuery<List<string>> responseValidationLac = new ResponseQuery<List<string>>();
                var requestValidationLac = new LacValidationDTO();
                requestValidationLac.UserId = request.UserId;
                responseValidationLac = await lacsValidationServices.ReadFileLacOrginal(requestValidationLac, responseValidationLac);
                if (!responseValidationLac.Success)
                {
                    response.SuccessData = false;
                    response.Message = $"Error in validation Lacs, {responseValidationLac.Message}";
                    response.Success = false;
                    return response;
                }

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

                ResponseQuery<List<string>> responseValidationSspd = new ResponseQuery<List<string>>();
                var requestValidationSspd = new LacValidationDTO();
                requestValidationSspd.UserId = request.UserId;
                responseValidationSspd = await SSPDValidationServices.ReadFileSspdOrginal(requestValidationSspd, responseValidationSspd);
                if (!responseValidationSspd.Success)
                {
                    response.SuccessData = false;
                    response.Message = $"Error in validation Lacs, {responseValidationSspd.Message}";
                    response.Success = false;
                    return response;
                }

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

                ResponseQuery<List<string>> responseValidationTc1 = new ResponseQuery<List<string>>();
                var requestValidationTc1 = new TC1ValidationDTO();
                requestValidationTc1.UserId = request.UserId;
                responseValidationTc1 = await TC1validationServices.ReadAssets(requestValidationTc1, responseValidationTc1);
                if (!responseValidationTc1.Success)
                {
                    response.SuccessData = false;
                    response.Message = $"Error in validation Lacs, {responseValidationTc1.Message}";
                    response.Success = false;
                    return response;
                }

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

    }
}
