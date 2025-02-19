using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IStatusFileEepDataAccess
    {

        public Task<Boolean> SaveDataList(List<StatusFile> request);

    }
}
