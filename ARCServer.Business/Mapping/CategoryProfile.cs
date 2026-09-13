using ARCServer.Business.Dtos.Categories;
using ARCServer.Domain.Entities;
using AutoMapper;

namespace ARCServer.Business.Mapping
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<CategoryTranslation, TranslationResultDto>();
            CreateMap<SubCategoryTranslation, TranslationResultDto>();

            CreateMap<SubCategory, SubCategoryDetailDto>()
                .ForMember(
                    dest => dest.Translations,
                    opt => opt.MapFrom(src => src.Translations
                        .Where(t => t.Deleted == 0)
                        .OrderBy(t => t.LanguageCode)));

            CreateMap<Category, CategoryDetailDto>()
                .ForMember(
                    dest => dest.Translations,
                    opt => opt.MapFrom(src => src.Translations
                        .Where(t => t.Deleted == 0)
                        .OrderBy(t => t.LanguageCode)))
                .ForMember(
                    dest => dest.SubCategories,
                    opt => opt.MapFrom(src => src.SubCategories
                        .Where(s => s.Deleted == 0)
                        .OrderBy(s => s.Id)));

            CreateMap<TranslationInputDto, CategoryTranslation>()
                .ForMember(
                    dest => dest.LanguageCode,
                    opt => opt.MapFrom(src => src.LanguageCode.Trim().ToLowerInvariant()))
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdaterId, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.DeletorId, opt => opt.Ignore())
                .ForMember(dest => dest.Deleted, opt => opt.Ignore());

            CreateMap<TranslationInputDto, SubCategoryTranslation>()
                .ForMember(
                    dest => dest.LanguageCode,
                    opt => opt.MapFrom(src => src.LanguageCode.Trim().ToLowerInvariant()))
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategoryId, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategory, opt => opt.Ignore())
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
