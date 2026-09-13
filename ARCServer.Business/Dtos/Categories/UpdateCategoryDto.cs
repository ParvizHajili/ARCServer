using Microsoft.AspNetCore.Http;

namespace ARCServer.Business.Dtos.Categories
{
    public class UpdateCategoryDto
    {
        /// <summary>
        /// Optional. Göndərilərsə Cloudinary-ə yüklənib yenilənir.
        /// </summary>
        public IFormFile? Image { get; set; }

        public int Order { get; set; }

        public List<TranslationInputDto> Translations { get; set; } = [];

        public List<UpdateSubCategoryDto> SubCategories { get; set; } = [];
    }

    public class UpdateSubCategoryDto
    {
        /// <summary>
        /// Existing subcategory id. Null means create a new subcategory.
        /// </summary>
        public int? Id { get; set; }

        public List<TranslationInputDto> Translations { get; set; } = [];
    }
}
