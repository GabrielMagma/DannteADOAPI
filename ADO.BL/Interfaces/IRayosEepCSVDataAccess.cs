using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IRayosEepCSVDataAccess
    {
        public Boolean SaveData(List<MpLightning> request);

    }
}
