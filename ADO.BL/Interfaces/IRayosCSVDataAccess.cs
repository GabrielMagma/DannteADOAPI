using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IRayosCSVDataAccess
    {

        public Boolean SaveData(List<MpLightning> request);

    }
}
