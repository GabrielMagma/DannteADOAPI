using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IStatusFileDataAccess
    {
        // assets
        public Task<Boolean> SaveDataAssetList(List<QueueStatusAsset> request);
        
        public Task<Boolean> UpdateDataAsset(QueueStatusAsset request);

        public Task<Boolean> UpdateDataAssetList(List<QueueStatusAsset> request);

        // ios
        public Task<Boolean> SaveDataIoList(List<QueueStatusIo> request);

        public Task<Boolean> UpdateDataIo(QueueStatusIo request);

        public Task<Boolean> UpdateDataIoList(List<QueueStatusIo> request);

        //lacs
        public Task<Boolean> SaveDataLACList(List<QueueStatusLac> request);

        public Task<Boolean> UpdateDataLAC(QueueStatusLac request);

        public Task<Boolean> UpdateDataLACList(List<QueueStatusLac> request);

        // sspd
        public Task<Boolean> SaveDataSSPDList(List<QueueStatusSspd> request);

        public Task<Boolean> UpdateDataSSPD(QueueStatusSspd request);

        public Task<Boolean> UpdateDataSSPDList(List<QueueStatusSspd> request);

        //tc1
        public Task<Boolean> SaveDataTC1List(List<QueueStatusTc1> request);

        public Task<Boolean> UpdateDataTC1(QueueStatusTc1 request);

        public Task<Boolean> UpdateDataTC1List(List<QueueStatusTc1> request);

        //tt2
        public Task<Boolean> SaveDataTT2List(List<QueueStatusTt2> request);

        public Task<Boolean> UpdateDataTT2(QueueStatusTt2 request);

        public Task<Boolean> UpdateDataTT2List(List<QueueStatusTt2> request);

        //poles
        public Task<Boolean> SaveDataPoleList(List<QueueStatusPole> request);

        public Task<Boolean> UpdateDataPole(QueueStatusPole request);

        public Task<Boolean> UpdateDataPoleList(List<QueueStatusPole> request);

    }
}
