
using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface ICompsDataAccess
    {

        public Task<Boolean> CreateFile(List<MpCompensation> request);

    }
}
