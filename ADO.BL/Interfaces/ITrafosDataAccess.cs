using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface ITrafosDataAccess
    {

        public Task<Boolean> SaveData(List<MpTransformerBurned> request);

    }
}
