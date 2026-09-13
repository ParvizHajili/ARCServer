using ARCServer.Business.Dtos.Categories;

namespace ARCServer.Business.Dtos.ManufacturerCountries
{
    public class CreateManufacturerCountryDto
    {
        public List<TranslationInputDto> Translations { get; set; } = [];
    }

    public class UpdateManufacturerCountryDto
    {
        public List<TranslationInputDto> Translations { get; set; } = [];
    }

    public class ManufacturerCountryDetailDto
    {
        public int Id { get; set; }

        public List<TranslationResultDto> Translations { get; set; } = [];
    }
}
