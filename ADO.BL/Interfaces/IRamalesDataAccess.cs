using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IRamalesDataAccess
    {

        public Boolean SaveData(List<FilesIo> request);

        //public Boolean SaveDataList(List<FileIoTempDetail> request);

    }
}
