using ARCServer.Business.Dtos.Categories;

namespace ARCServer.Business.Dtos.Brands
{
    public class CreateBrandDto
    {
        public List<TranslationInputDto> Translations { get; set; } = [];
    }

    public class UpdateBrandDto
    {
        public List<TranslationInputDto> Translations { get; set; } = [];
    }

    public class BrandDetailDto
    {
        public int Id { get; set; }

        public List<TranslationResultDto> Translations { get; set; } = [];
    }
}
