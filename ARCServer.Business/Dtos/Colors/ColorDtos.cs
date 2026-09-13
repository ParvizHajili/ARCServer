using ARCServer.Business.Dtos.Categories;

namespace ARCServer.Business.Dtos.Colors
{
    public class CreateColorDto
    {
        public string HexCode { get; set; } = string.Empty;

        public List<TranslationInputDto> Translations { get; set; } = [];
    }

    public class UpdateColorDto
    {
        public string HexCode { get; set; } = string.Empty;

        public List<TranslationInputDto> Translations { get; set; } = [];
    }

    public class ColorDetailDto
    {
        public int Id { get; set; }

        public string HexCode { get; set; } = string.Empty;

        public List<TranslationResultDto> Translations { get; set; } = [];
    }
}
