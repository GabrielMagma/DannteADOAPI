﻿
using ADO.BL.DataEntities;

namespace ADO.BL.Interfaces
{
    public interface IFileIODataAccess
    {

        public Task SaveData(List<FilesIo> request);

        public Task SaveDataComplete(List<FilesIoComplete> request); 

        public Task DeleteData(string fileName);

    }
}
