
using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IPolesEepDataAccess
    {

        public Task<Boolean> CreateFile(List<MpUtilityPole> request);

    }
}
