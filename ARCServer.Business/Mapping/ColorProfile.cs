using ARCServer.Business.Dtos.Categories;
using ARCServer.Business.Dtos.Colors;
using ARCServer.Business.Validators.Colors;
using ARCServer.Domain.Entities;
using AutoMapper;

namespace ARCServer.Business.Mapping
{
    public class ColorProfile : Profile
    {
        public ColorProfile()
        {
            CreateMap<ColorTranslation, TranslationResultDto>();

            CreateMap<Color, ColorDetailDto>()
                .ForMember(
                    dest => dest.Translations,
                    opt => opt.MapFrom(src => src.Translations
                        .Where(t => t.Deleted == 0)
                        .OrderBy(t => t.LanguageCode)));

            CreateMap<TranslationInputDto, ColorTranslation>()
                .ForMember(
                    dest => dest.LanguageCode,
                    opt => opt.MapFrom(src => src.LanguageCode.Trim().ToLowerInvariant()))
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ColorId, opt => opt.Ignore())
                .ForMember(dest => dest.Color, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdaterId, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.DeletorId, opt => opt.Ignore())
                .ForMember(dest => dest.Deleted, opt => opt.Ignore());

            CreateMap<CreateColorDto, Color>()
                .ForMember(
                    dest => dest.HexCode,
                    opt => opt.MapFrom(src => ColorHexRules.Normalize(src.HexCode)))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Translations, opt => opt.Ignore())
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
