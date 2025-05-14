
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
            CreateMap<StatusFileDTO, QueueStatusPole>().ReverseMap();
            CreateMap<StatusFileDTO, QueueStatusPoda>().ReverseMap();
            CreateMap<StatusFileDTO, QueueStatusCompensation>().ReverseMap();
            CreateMap<StatusFileDTO, QueueStatusTransformerBurned>().ReverseMap();
            CreateMap<MpLightningDTO, MpLightning>().ReverseMap();
            CreateMap<MpCompensacionesDTO, MpCompensation>().ReverseMap();
            CreateMap<PodaDTO, IaPoda>().ReverseMap();
            CreateMap<FileIoCompleteDTO, FilesIoComplete>().ReverseMap();
            CreateMap<FileIoDTO, FilesIo>().ReverseMap();
            CreateMap<FileIoTempDTO, FileIoTemp>().ReverseMap();
            CreateMap<MpUtilityPoleDTO, MpUtilityPole>().ReverseMap();
            CreateMap<MpTransformerBurnedDTO, MpTransformerBurned>().ReverseMap();
        }
    }
}
