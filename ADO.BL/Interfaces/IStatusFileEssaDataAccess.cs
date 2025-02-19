using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IStatusFileEssaDataAccess
    {

        public Task<Boolean> SaveDataList(List<StatusFile> request);

    }
}
