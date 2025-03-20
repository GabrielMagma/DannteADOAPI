using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IPodasEssaDataAccess
    {

        public Boolean SaveData(List<IaPoda> request);

    }
}
