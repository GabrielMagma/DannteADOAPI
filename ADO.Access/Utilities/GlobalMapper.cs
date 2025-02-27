
using ADO.BL.DataEntities;
using ADO.BL.DTOs;
using AutoMapper;

namespace ADO.Access
{
    public class GlobalMapper : Profile
    {

        public GlobalMapper()
        {
            CreateMap<IdeamDTO, Ideam>().ReverseMap();
            CreateMap<AllAssetDTO, AllAsset>().ReverseMap();
            CreateMap<AllAssetDTO, AllAssetNew>().ReverseMap();
            CreateMap<StatusFileDTO, StatusFile>().ReverseMap();
            CreateMap<MpLightningDTO, MpLightning>().ReverseMap();
            CreateMap<PodaDTO, Poda>().ReverseMap();

        }
    }
}
