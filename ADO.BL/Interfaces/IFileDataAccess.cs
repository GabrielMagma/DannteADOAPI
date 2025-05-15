
using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IFileDataAccess
    {

        public Task<Boolean> CreateFile(List<IaIdeam> request);

    }
}
