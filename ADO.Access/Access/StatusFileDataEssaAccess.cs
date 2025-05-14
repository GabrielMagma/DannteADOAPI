using ADO.Access.DataTest;
using ADO.BL.DataEntities;
using ADO.BL.Interfaces;
using AutoMapper;

namespace ADO.Access.Access
{
    public class StatusFileDataEssaAccess : IStatusFileDataAccess
    {
        protected DannteTestingContext context;
        private readonly IMapper mapper;

        public StatusFileDataEssaAccess(DannteTestingContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }

        //asset
        public async Task<Boolean> SaveDataAssetList(List<QueueStatusAsset> request)
        {
            
            context.QueueStatusAssets.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public async Task<Boolean> UpdateDataAsset(QueueStatusAsset request)
        {

            var assetExist = context.QueueStatusAssets.FirstOrDefault(x => x.FileName == request.FileName);

            if (assetExist != null)
            {
                //actualizar data
                //FrameworkTypeUtility.SetProperties(request, assetExist);

                assetExist.UserId = request.UserId == null ? assetExist.UserId : request.UserId;
                assetExist.FileType = request.FileType == null ? assetExist.FileType : request.FileType;
                assetExist.DateFile = request.DateFile == null ? assetExist.DateFile : request.DateFile;
                assetExist.Year = request.Year == null ? assetExist.Year : request.Year;
                assetExist.Month = request.Month == null ? assetExist.Month : request.Month;
                assetExist.Day = request.Day == null ? assetExist.Day : request.Day;
                assetExist.Status = request.Status == null ? assetExist.Status : request.Status;
                assetExist.DateRegister = request.DateRegister == null ? assetExist.DateRegister : request.DateRegister;

                //guardar cambios                
                context.SaveChanges();
                var result = true;

                return result;
            }
            else
            {
                var result = false;

                return result;
            }


        }

        public async Task<Boolean> UpdateDataAssetList(List<QueueStatusAsset> request)
        {
            var result = true;
            foreach (var item in request)
            {
                var assetExist = context.QueueStatusAssets.FirstOrDefault(x => x.FileName == item.FileName);

                if (assetExist != null)
                {
                    //actualizar data
                    //FrameworkTypeUtility.SetProperties(item, assetExist);

                    assetExist.UserId = item.UserId == null ? assetExist.UserId : item.UserId;
                    assetExist.FileType = item.FileType == null ? assetExist.FileType : item.FileType;
                    assetExist.DateFile = item.DateFile == null ? assetExist.DateFile : item.DateFile;
                    assetExist.Year = item.Year == null ? assetExist.Year : item.Year;
                    assetExist.Month = item.Month == null ? assetExist.Month : item.Month;
                    assetExist.Day = item.Day == null ? assetExist.Day : item.Day;
                    assetExist.Status = item.Status == null ? assetExist.Status : item.Status;
                    assetExist.DateRegister = item.DateRegister == null ? assetExist.DateRegister : item.DateRegister;

                    //guardar cambios                
                    context.SaveChanges();
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;

        }

        //io
        public async Task<Boolean> SaveDataIoList(List<QueueStatusIo> request)
        {

            context.QueueStatusIos.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public async Task<Boolean> UpdateDataIo(QueueStatusIo request)
        {

            var ioExist = context.QueueStatusIos.FirstOrDefault(x => x.FileName == request.FileName);

            if (ioExist != null)
            {
                //actualizar data
                //FrameworkTypeUtility.SetProperties(request, ioExist);

                ioExist.UserId = request.UserId == null ? ioExist.UserId : request.UserId;
                ioExist.FileType = request.FileType == null ? ioExist.FileType : request.FileType;
                ioExist.DateFile = request.DateFile == null ? ioExist.DateFile : request.DateFile;
                ioExist.Year = request.Year == null ? ioExist.Year : request.Year;
                ioExist.Month = request.Month == null ? ioExist.Month : request.Month;
                ioExist.Day = request.Day == null ? ioExist.Day : request.Day;
                ioExist.Status = request.Status == null ? ioExist.Status : request.Status;
                ioExist.DateRegister = request.DateRegister == null ? ioExist.DateRegister : request.DateRegister;

                //guardar cambios                
                context.SaveChanges();
                var result = true;

                return result;
            }
            else
            {
                var result = false;

                return result;
            }


        }

        public async Task<Boolean> UpdateDataIoList(List<QueueStatusIo> request)
        {
            var result = true;
            foreach (var item in request)
            {
                var ioExist = context.QueueStatusIos.FirstOrDefault(x => x.FileName == item.FileName);

                if (ioExist != null)
                {
                    //actualizar data
                    //FrameworkTypeUtility.SetProperties(item, ioExist);

                    ioExist.UserId = item.UserId == null ? ioExist.UserId : item.UserId;
                    ioExist.FileType = item.FileType == null ? ioExist.FileType : item.FileType;
                    ioExist.DateFile = item.DateFile == null ? ioExist.DateFile : item.DateFile;
                    ioExist.Year = item.Year == null ? ioExist.Year : item.Year;
                    ioExist.Month = item.Month == null ? ioExist.Month : item.Month;
                    ioExist.Day = item.Day == null ? ioExist.Day : item.Day;
                    ioExist.Status = item.Status == null ? ioExist.Status : item.Status;
                    ioExist.DateRegister = item.DateRegister == null ? ioExist.DateRegister : item.DateRegister;

                    //guardar cambios                
                    context.SaveChanges();
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;

        }

        //lac
        public async Task<Boolean> SaveDataLACList(List<QueueStatusLac> request)
        {

            context.QueueStatusLacs.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public async Task<Boolean> UpdateDataLAC(QueueStatusLac request)
        {

            var lacExist = context.QueueStatusLacs.FirstOrDefault(x => x.FileName == request.FileName);

            if (lacExist != null)
            {
                //actualizar data
                //FrameworkTypeUtility.SetProperties(request, lacExist);

                lacExist.UserId = request.UserId == null ? lacExist.UserId : request.UserId;
                lacExist.FileType = request.FileType == null ? lacExist.FileType : request.FileType;
                lacExist.DateFile = request.DateFile == null ? lacExist.DateFile : request.DateFile;
                lacExist.Year = request.Year == null ? lacExist.Year : request.Year;
                lacExist.Month = request.Month == null ? lacExist.Month : request.Month;
                lacExist.Day = request.Day == null ? lacExist.Day : request.Day;
                lacExist.Status = request.Status == null ? lacExist.Status : request.Status;
                lacExist.DateRegister = request.DateRegister == null ? lacExist.DateRegister : request.DateRegister;

                //guardar cambios                
                context.SaveChanges();
                var result = true;

                return result;
            }
            else
            {
                var result = false;

                return result;
            }

            
        }

        public async Task<Boolean> UpdateDataLACList(List<QueueStatusLac> request)
        {
            var result = true;
            foreach (var item in request)
            {
                var lacExist = context.QueueStatusLacs.FirstOrDefault(x => x.FileName == item.FileName);

                if (lacExist != null)
                {
                    //actualizar data
                    //FrameworkTypeUtility.SetProperties(item, lacExist);

                    lacExist.UserId = item.UserId == null ? lacExist.UserId : item.UserId;
                    lacExist.FileType = item.FileType == null ? lacExist.FileType : item.FileType;
                    lacExist.DateFile = item.DateFile == null ? lacExist.DateFile : item.DateFile;
                    lacExist.Year = item.Year == null ? lacExist.Year : item.Year;
                    lacExist.Month = item.Month == null ? lacExist.Month : item.Month;
                    lacExist.Day = item.Day == null ? lacExist.Day : item.Day;
                    lacExist.Status = item.Status == null ? lacExist.Status : item.Status;
                    lacExist.DateRegister = item.DateRegister == null ? lacExist.DateRegister : item.DateRegister;

                    //guardar cambios                
                    context.SaveChanges();
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;

        }

        //sspd
        public async Task<Boolean> SaveDataSSPDList(List<QueueStatusSspd> request)
        {

            context.QueueStatusSspds.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public async Task<Boolean> UpdateDataSSPD(QueueStatusSspd request)
        {

            var sspdExist = context.QueueStatusSspds.FirstOrDefault(x => x.FileName == request.FileName);

            if (sspdExist != null)
            {
                //actualizar data
                //FrameworkTypeUtility.SetProperties(request, sspdExist);

                sspdExist.UserId = request.UserId == null ? sspdExist.UserId : request.UserId;
                sspdExist.FileType = request.FileType == null ? sspdExist.FileType : request.FileType;
                sspdExist.DateFile = request.DateFile == null ? sspdExist.DateFile : request.DateFile;
                sspdExist.Year = request.Year == null ? sspdExist.Year : request.Year;
                sspdExist.Month = request.Month == null ? sspdExist.Month : request.Month;
                sspdExist.Day = request.Day == null ? sspdExist.Day : request.Day;
                sspdExist.Status = request.Status == null ? sspdExist.Status : request.Status;
                sspdExist.DateRegister = request.DateRegister == null ? sspdExist.DateRegister : request.DateRegister;

                //guardar cambios                
                context.SaveChanges();
                var result = true;

                return result;
            }
            else
            {
                var result = false;

                return result;
            }


        }

        public async Task<Boolean> UpdateDataSSPDList(List<QueueStatusSspd> request)
        {
            var result = true;
            foreach (var item in request)
            {
                var sspdExist = context.QueueStatusSspds.FirstOrDefault(x => x.FileName == item.FileName);

                if (sspdExist != null)
                {
                    //actualizar data
                    //FrameworkTypeUtility.SetProperties(item, sspdExist);

                    sspdExist.UserId = item.UserId == null ? sspdExist.UserId : item.UserId;
                    sspdExist.FileType = item.FileType == null ? sspdExist.FileType : item.FileType;
                    sspdExist.DateFile = item.DateFile == null ? sspdExist.DateFile : item.DateFile;
                    sspdExist.Year = item.Year == null ? sspdExist.Year : item.Year;
                    sspdExist.Month = item.Month == null ? sspdExist.Month : item.Month;
                    sspdExist.Day = item.Day == null ? sspdExist.Day : item.Day;
                    sspdExist.Status = item.Status == null ? sspdExist.Status : item.Status;
                    sspdExist.DateRegister = item.DateRegister == null ? sspdExist.DateRegister : item.DateRegister;

                    //guardar cambios                
                    context.SaveChanges();
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;

        }

        //tc1
        public async Task<Boolean> SaveDataTC1List(List<QueueStatusTc1> request)
        {

            context.QueueStatusTc1s.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public async Task<Boolean> UpdateDataTC1(QueueStatusTc1 request)
        {

            var tc1Exist = context.QueueStatusTc1s.FirstOrDefault(x => x.FileName == request.FileName);

            if (tc1Exist != null)
            {
                //actualizar data
                //FrameworkTypeUtility.SetProperties(request, tc1Exist);

                tc1Exist.UserId = request.UserId == null ? tc1Exist.UserId : request.UserId;
                tc1Exist.FileType = request.FileType == null ? tc1Exist.FileType : request.FileType;
                tc1Exist.DateFile = request.DateFile == null ? tc1Exist.DateFile : request.DateFile;
                tc1Exist.Year = request.Year == null ? tc1Exist.Year : request.Year;
                tc1Exist.Month = request.Month == null ? tc1Exist.Month : request.Month;
                tc1Exist.Day = request.Day == null ? tc1Exist.Day : request.Day;
                tc1Exist.Status = request.Status == null ? tc1Exist.Status : request.Status;
                tc1Exist.DateRegister = request.DateRegister == null ? tc1Exist.DateRegister : request.DateRegister;

                //guardar cambios                
                context.SaveChanges();
                var result = true;

                return result;
            }
            else
            {
                var result = false;

                return result;
            }


        }

        public async Task<Boolean> UpdateDataTC1List(List<QueueStatusTc1> request)
        {
            var result = true;
            foreach (var item in request)
            {
                var tc1Exist = context.QueueStatusTc1s.FirstOrDefault(x => x.FileName == item.FileName);

                if (tc1Exist != null)
                {
                    //actualizar data
                    //FrameworkTypeUtility.SetProperties(item, tc1Exist);

                    tc1Exist.UserId = item.UserId == null ? tc1Exist.UserId : item.UserId;
                    tc1Exist.FileType = item.FileType == null ? tc1Exist.FileType : item.FileType;
                    tc1Exist.DateFile = item.DateFile == null ? tc1Exist.DateFile : item.DateFile;
                    tc1Exist.Year = item.Year == null ? tc1Exist.Year : item.Year;
                    tc1Exist.Month = item.Month == null ? tc1Exist.Month : item.Month;
                    tc1Exist.Day = item.Day == null ? tc1Exist.Day : item.Day;
                    tc1Exist.Status = item.Status == null ? tc1Exist.Status : item.Status;
                    tc1Exist.DateRegister = item.DateRegister == null ? tc1Exist.DateRegister : item.DateRegister;

                    //guardar cambios                
                    context.SaveChanges();
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;

        }

        //tt2
        public async Task<Boolean> SaveDataTT2List(List<QueueStatusTt2> request)
        {

            context.QueueStatusTt2s.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public async Task<Boolean> UpdateDataTT2(QueueStatusTt2 request)
        {

            var tt2Exist = context.QueueStatusTt2s.FirstOrDefault(x => x.FileName == request.FileName);

            if (tt2Exist != null)
            {
                //actualizar data
                //FrameworkTypeUtility.SetProperties(request, tt2Exist);

                tt2Exist.UserId = request.UserId == null ? tt2Exist.UserId : request.UserId;
                tt2Exist.FileType = request.FileType == null ? tt2Exist.FileType : request.FileType;
                tt2Exist.DateFile = request.DateFile == null ? tt2Exist.DateFile : request.DateFile;
                tt2Exist.Year = request.Year == null ? tt2Exist.Year : request.Year;
                tt2Exist.Month = request.Month == null ? tt2Exist.Month : request.Month;
                tt2Exist.Day = request.Day == null ? tt2Exist.Day : request.Day;
                tt2Exist.Status = request.Status == null ? tt2Exist.Status : request.Status;
                tt2Exist.DateRegister = request.DateRegister == null ? tt2Exist.DateRegister : request.DateRegister;

                //guardar cambios                
                context.SaveChanges();
                var result = true;

                return result;
            }
            else
            {
                var result = false;

                return result;
            }


        }

        public async Task<Boolean> UpdateDataTT2List(List<QueueStatusTt2> request)
        {
            var result = true;
            foreach (var item in request)
            {
                var tt2Exist = context.QueueStatusTt2s.FirstOrDefault(x => x.FileName == item.FileName);

                if (tt2Exist != null)
                {
                    //actualizar data
                    //FrameworkTypeUtility.SetProperties(item, tt2Exist);

                    tt2Exist.UserId = item.UserId == null ? tt2Exist.UserId : item.UserId;
                    tt2Exist.FileType = item.FileType == null ? tt2Exist.FileType : item.FileType;
                    tt2Exist.DateFile = item.DateFile == null ? tt2Exist.DateFile : item.DateFile;
                    tt2Exist.Year = item.Year == null ? tt2Exist.Year : item.Year;
                    tt2Exist.Month = item.Month == null ? tt2Exist.Month : item.Month;
                    tt2Exist.Day = item.Day == null ? tt2Exist.Day : item.Day;
                    tt2Exist.Status = item.Status == null ? tt2Exist.Status : item.Status;
                    tt2Exist.DateRegister = item.DateRegister == null ? tt2Exist.DateRegister : item.DateRegister;

                    //guardar cambios                
                    context.SaveChanges();
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;

        }

        //Poles
        public async Task<Boolean> SaveDataPoleList(List<QueueStatusPole> request)
        {

            context.QueueStatusPoles.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public async Task<Boolean> UpdateDataPole(QueueStatusPole request)
        {

            var poleExist = context.QueueStatusPoles.FirstOrDefault(x => x.FileName == request.FileName);

            if (poleExist != null)
            {
                //actualizar data
                //FrameworkTypeUtility.SetProperties(request, tt2Exist);

                poleExist.UserId = request.UserId == null ? poleExist.UserId : request.UserId;
                poleExist.FileType = request.FileType == null ? poleExist.FileType : request.FileType;
                poleExist.DateFile = request.DateFile == null ? poleExist.DateFile : request.DateFile;
                poleExist.Year = request.Year == null ? poleExist.Year : request.Year;
                poleExist.Month = request.Month == null ? poleExist.Month : request.Month;
                poleExist.Day = request.Day == null ? poleExist.Day : request.Day;
                poleExist.Status = request.Status == null ? poleExist.Status : request.Status;
                poleExist.DateRegister = request.DateRegister == null ? poleExist.DateRegister : request.DateRegister;

                //guardar cambios                
                context.SaveChanges();
                var result = true;

                return result;
            }
            else
            {
                var result = false;

                return result;
            }


        }

        public async Task<Boolean> UpdateDataPoleList(List<QueueStatusPole> request)
        {
            var result = true;
            foreach (var item in request)
            {
                var poleExist = context.QueueStatusTt2s.FirstOrDefault(x => x.FileName == item.FileName);

                if (poleExist != null)
                {
                    //actualizar data
                    //FrameworkTypeUtility.SetProperties(item, tt2Exist);

                    poleExist.UserId = item.UserId == null ? poleExist.UserId : item.UserId;
                    poleExist.FileType = item.FileType == null ? poleExist.FileType : item.FileType;
                    poleExist.DateFile = item.DateFile == null ? poleExist.DateFile : item.DateFile;
                    poleExist.Year = item.Year == null ? poleExist.Year : item.Year;
                    poleExist.Month = item.Month == null ? poleExist.Month : item.Month;
                    poleExist.Day = item.Day == null ? poleExist.Day : item.Day;
                    poleExist.Status = item.Status == null ? poleExist.Status : item.Status;
                    poleExist.DateRegister = item.DateRegister == null ? poleExist.DateRegister : item.DateRegister;

                    //guardar cambios                
                    context.SaveChanges();
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;

        }

        //Rayos
        public async Task<Boolean> SaveDataRayosList(List<QueueStatusLightning> request)
        {

            context.QueueStatusLightnings.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public async Task<Boolean> UpdateDataRayos(QueueStatusLightning request)
        {

            var assetExist = context.QueueStatusLightnings.FirstOrDefault(x => x.FileName == request.FileName);

            if (assetExist != null)
            {
                //actualizar data
                //FrameworkTypeUtility.SetProperties(request, assetExist);

                assetExist.UserId = request.UserId == null ? assetExist.UserId : request.UserId;
                assetExist.FileType = request.FileType == null ? assetExist.FileType : request.FileType;
                assetExist.DateFile = request.DateFile == null ? assetExist.DateFile : request.DateFile;
                assetExist.Year = request.Year == null ? assetExist.Year : request.Year;
                assetExist.Month = request.Month == null ? assetExist.Month : request.Month;
                assetExist.Day = request.Day == null ? assetExist.Day : request.Day;
                assetExist.Status = request.Status == null ? assetExist.Status : request.Status;
                assetExist.DateRegister = request.DateRegister == null ? assetExist.DateRegister : request.DateRegister;

                //guardar cambios                
                context.SaveChanges();
                var result = true;

                return result;
            }
            else
            {
                var result = false;

                return result;
            }


        }

        public async Task<Boolean> UpdateDataRayosList(List<QueueStatusLightning> request)
        {
            var result = true;
            foreach (var item in request)
            {
                var assetExist = context.QueueStatusLightnings.FirstOrDefault(x => x.FileName == item.FileName);

                if (assetExist != null)
                {
                    //actualizar data
                    //FrameworkTypeUtility.SetProperties(item, assetExist);

                    assetExist.UserId = item.UserId == null ? assetExist.UserId : item.UserId;
                    assetExist.FileType = item.FileType == null ? assetExist.FileType : item.FileType;
                    assetExist.DateFile = item.DateFile == null ? assetExist.DateFile : item.DateFile;
                    assetExist.Year = item.Year == null ? assetExist.Year : item.Year;
                    assetExist.Month = item.Month == null ? assetExist.Month : item.Month;
                    assetExist.Day = item.Day == null ? assetExist.Day : item.Day;
                    assetExist.Status = item.Status == null ? assetExist.Status : item.Status;
                    assetExist.DateRegister = item.DateRegister == null ? assetExist.DateRegister : item.DateRegister;

                    //guardar cambios                
                    context.SaveChanges();
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;

        }

        //Compensaciones
        public async Task<Boolean> SaveDataCompensationsList(List<QueueStatusCompensation> request)
        {

            context.QueueStatusCompensations.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public async Task<Boolean> UpdateDataCompensations(QueueStatusCompensation request)
        {

            var assetExist = context.QueueStatusCompensations.FirstOrDefault(x => x.FileName == request.FileName);

            if (assetExist != null)
            {
                //actualizar data
                //FrameworkTypeUtility.SetProperties(request, assetExist);

                assetExist.UserId = request.UserId == null ? assetExist.UserId : request.UserId;
                assetExist.FileType = request.FileType == null ? assetExist.FileType : request.FileType;
                assetExist.DateFile = request.DateFile == null ? assetExist.DateFile : request.DateFile;
                assetExist.Year = request.Year == null ? assetExist.Year : request.Year;
                assetExist.Month = request.Month == null ? assetExist.Month : request.Month;
                assetExist.Day = request.Day == null ? assetExist.Day : request.Day;
                assetExist.Status = request.Status == null ? assetExist.Status : request.Status;
                assetExist.DateRegister = request.DateRegister == null ? assetExist.DateRegister : request.DateRegister;

                //guardar cambios                
                context.SaveChanges();
                var result = true;

                return result;
            }
            else
            {
                var result = false;

                return result;
            }


        }

        public async Task<Boolean> UpdateDataCompensationsList(List<QueueStatusCompensation> request)
        {
            var result = true;
            foreach (var item in request)
            {
                var assetExist = context.QueueStatusCompensations.FirstOrDefault(x => x.FileName == item.FileName);

                if (assetExist != null)
                {
                    //actualizar data
                    //FrameworkTypeUtility.SetProperties(item, assetExist);

                    assetExist.UserId = item.UserId == null ? assetExist.UserId : item.UserId;
                    assetExist.FileType = item.FileType == null ? assetExist.FileType : item.FileType;
                    assetExist.DateFile = item.DateFile == null ? assetExist.DateFile : item.DateFile;
                    assetExist.Year = item.Year == null ? assetExist.Year : item.Year;
                    assetExist.Month = item.Month == null ? assetExist.Month : item.Month;
                    assetExist.Day = item.Day == null ? assetExist.Day : item.Day;
                    assetExist.Status = item.Status == null ? assetExist.Status : item.Status;
                    assetExist.DateRegister = item.DateRegister == null ? assetExist.DateRegister : item.DateRegister;

                    //guardar cambios                
                    context.SaveChanges();
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;

        }

        //Podas
        public async Task<Boolean> SaveDataPodasList(List<QueueStatusPoda> request)
        {

            context.QueueStatusPodas.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public async Task<Boolean> UpdateDataPodas(QueueStatusPoda request)
        {

            var assetExist = context.QueueStatusPodas.FirstOrDefault(x => x.FileName == request.FileName);

            if (assetExist != null)
            {
                //actualizar data
                //FrameworkTypeUtility.SetProperties(request, assetExist);

                assetExist.UserId = request.UserId == null ? assetExist.UserId : request.UserId;
                assetExist.FileType = request.FileType == null ? assetExist.FileType : request.FileType;
                assetExist.DateFile = request.DateFile == null ? assetExist.DateFile : request.DateFile;
                assetExist.Year = request.Year == null ? assetExist.Year : request.Year;
                assetExist.Month = request.Month == null ? assetExist.Month : request.Month;
                assetExist.Day = request.Day == null ? assetExist.Day : request.Day;
                assetExist.Status = request.Status == null ? assetExist.Status : request.Status;
                assetExist.DateRegister = request.DateRegister == null ? assetExist.DateRegister : request.DateRegister;

                //guardar cambios                
                context.SaveChanges();
                var result = true;

                return result;
            }
            else
            {
                var result = false;

                return result;
            }


        }

        public async Task<Boolean> UpdateDataPodasList(List<QueueStatusPoda> request)
        {
            var result = true;
            foreach (var item in request)
            {
                var assetExist = context.QueueStatusPodas.FirstOrDefault(x => x.FileName == item.FileName);

                if (assetExist != null)
                {
                    //actualizar data
                    //FrameworkTypeUtility.SetProperties(item, assetExist);

                    assetExist.UserId = item.UserId == null ? assetExist.UserId : item.UserId;
                    assetExist.FileType = item.FileType == null ? assetExist.FileType : item.FileType;
                    assetExist.DateFile = item.DateFile == null ? assetExist.DateFile : item.DateFile;
                    assetExist.Year = item.Year == null ? assetExist.Year : item.Year;
                    assetExist.Month = item.Month == null ? assetExist.Month : item.Month;
                    assetExist.Day = item.Day == null ? assetExist.Day : item.Day;
                    assetExist.Status = item.Status == null ? assetExist.Status : item.Status;
                    assetExist.DateRegister = item.DateRegister == null ? assetExist.DateRegister : item.DateRegister;

                    //guardar cambios                
                    context.SaveChanges();
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;

        }

        //Trafos q
        public async Task<Boolean> SaveDataTrafosQuemadosList(List<QueueStatusTransformerBurned> request)
        {

            context.QueueStatusTransformerBurneds.AddRange(request);
            context.SaveChanges();
            var result = true;

            return result;
        }

        public async Task<Boolean> UpdateDataTrafosQuemados(QueueStatusTransformerBurned request)
        {

            var assetExist = context.QueueStatusTransformerBurneds.FirstOrDefault(x => x.FileName == request.FileName);

            if (assetExist != null)
            {
                //actualizar data
                //FrameworkTypeUtility.SetProperties(request, assetExist);

                assetExist.UserId = request.UserId == null ? assetExist.UserId : request.UserId;
                assetExist.FileType = request.FileType == null ? assetExist.FileType : request.FileType;
                assetExist.DateFile = request.DateFile == null ? assetExist.DateFile : request.DateFile;
                assetExist.Year = request.Year == null ? assetExist.Year : request.Year;
                assetExist.Month = request.Month == null ? assetExist.Month : request.Month;
                assetExist.Day = request.Day == null ? assetExist.Day : request.Day;
                assetExist.Status = request.Status == null ? assetExist.Status : request.Status;
                assetExist.DateRegister = request.DateRegister == null ? assetExist.DateRegister : request.DateRegister;

                //guardar cambios                
                context.SaveChanges();
                var result = true;

                return result;
            }
            else
            {
                var result = false;

                return result;
            }


        }

        public async Task<Boolean> UpdateDataTrafosQuemadosList(List<QueueStatusTransformerBurned> request)
        {
            var result = true;
            foreach (var item in request)
            {
                var assetExist = context.QueueStatusTransformerBurneds.FirstOrDefault(x => x.FileName == item.FileName);

                if (assetExist != null)
                {
                    //actualizar data
                    //FrameworkTypeUtility.SetProperties(item, assetExist);

                    assetExist.UserId = item.UserId == null ? assetExist.UserId : item.UserId;
                    assetExist.FileType = item.FileType == null ? assetExist.FileType : item.FileType;
                    assetExist.DateFile = item.DateFile == null ? assetExist.DateFile : item.DateFile;
                    assetExist.Year = item.Year == null ? assetExist.Year : item.Year;
                    assetExist.Month = item.Month == null ? assetExist.Month : item.Month;
                    assetExist.Day = item.Day == null ? assetExist.Day : item.Day;
                    assetExist.Status = item.Status == null ? assetExist.Status : item.Status;
                    assetExist.DateRegister = item.DateRegister == null ? assetExist.DateRegister : item.DateRegister;

                    //guardar cambios                
                    context.SaveChanges();
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;

        }

    }
}
