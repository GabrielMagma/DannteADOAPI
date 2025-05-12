
using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using AutoMapper;

namespace ADO.Access
{
    public class GlobalMapper : Profile
    {
        public GlobalMapper()
        {
            CreateMap<IdeamDTO, IaIdeam>().ReverseMap();
            CreateMap<AllAssetDTO, AllAsset>().ReverseMap();
            CreateMap<AllAssetDTO, AllAssetNew>().ReverseMap();
            CreateMap<StatusFileDTO, QueueStatusAsset>().ReverseMap();
            CreateMap<StatusFileDTO, QueueStatusIo>().ReverseMap();
            CreateMap<StatusFileDTO, QueueStatusLac>().ReverseMap();
            CreateMap<StatusFileDTO, QueueStatusSspd>().ReverseMap();
            CreateMap<StatusFileDTO, QueueStatusTc1>().ReverseMap();
            CreateMap<StatusFileDTO, QueueStatusTt2>().ReverseMap();
            CreateMap<StatusFileDTO, QueueStatusLightning>().ReverseMap();
            CreateMap<MpLightningDTO, MpLightning>().ReverseMap();
            CreateMap<PodaDTO, IaPoda>().ReverseMap();
            CreateMap<FileIoCompleteDTO, FilesIoComplete>().ReverseMap();
            CreateMap<FileIoDTO, FilesIo>().ReverseMap();
            CreateMap<FileIoTempDTO, FileIoTemp>().ReverseMap();
            CreateMap<MpUtilityPoleDTO, MpUtilityPole>().ReverseMap();
        }
    }
}
