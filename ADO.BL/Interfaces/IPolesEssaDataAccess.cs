
using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IPolesEssaDataAccess
    {

        public Task<Boolean> CreateFile(List<MpUtilityPole> request);

    }
}
