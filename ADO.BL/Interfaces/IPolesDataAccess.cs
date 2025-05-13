
using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IPolesDataAccess
    {

        public Task<Boolean> CreateFile(List<MpUtilityPole> request);

    }
}
