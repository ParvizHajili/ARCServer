using ARCServer.Business.Dtos.Categories;
using ARCServer.Business.Dtos.Brands;
using ARCServer.Domain.Entities;
using AutoMapper;

namespace ARCServer.Business.Mapping
{
    public class BrandProfile : Profile
    {
        public BrandProfile()
        {
            CreateMap<BrandTranslation, TranslationResultDto>();

            CreateMap<Brand, BrandDetailDto>()
                .ForMember(
                    dest => dest.Translations,
                    opt => opt.MapFrom(src => src.Translations
                        .Where(t => t.Deleted == 0)
                        .OrderBy(t => t.LanguageCode)));

            CreateMap<TranslationInputDto, BrandTranslation>()
                .ForMember(
                    dest => dest.LanguageCode,
                    opt => opt.MapFrom(src => src.LanguageCode.Trim().ToLowerInvariant()))
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BrandId, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdaterId, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.DeletorId, opt => opt.Ignore())
                .ForMember(dest => dest.Deleted, opt => opt.Ignore());
        }
    }
}
