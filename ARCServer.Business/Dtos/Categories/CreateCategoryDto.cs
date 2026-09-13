using Microsoft.AspNetCore.Http;

namespace ARCServer.Business.Dtos.Categories
{
    public class CreateCategoryDto
    {
        public IFormFile? Image { get; set; }

        public int Order { get; set; }

        public List<TranslationInputDto> Translations { get; set; } = [];

        public List<CreateSubCategoryDto> SubCategories { get; set; } = [];
    }
}
