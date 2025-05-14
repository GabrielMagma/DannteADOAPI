using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IPodasDataAccess
    {

        public Task<Boolean> SaveData(List<IaPoda> request);

    }
}
