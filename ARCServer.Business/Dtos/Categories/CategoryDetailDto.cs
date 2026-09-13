namespace ARCServer.Business.Dtos.Categories
{
    public class CategoryDetailDto
    {
        public int Id { get; set; }

        public string Image { get; set; } = string.Empty;

        public int Order { get; set; }

        public List<TranslationResultDto> Translations { get; set; } = [];

        public List<SubCategoryDetailDto> SubCategories { get; set; } = [];
    }

    public class TranslationResultDto
    {
        public string LanguageCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }

    public class SubCategoryDetailDto
    {
        public int Id { get; set; }

        public List<TranslationResultDto> Translations { get; set; } = [];
    }
}
