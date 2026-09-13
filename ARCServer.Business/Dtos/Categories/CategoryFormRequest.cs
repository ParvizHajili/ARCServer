using Microsoft.AspNetCore.Http;

namespace ARCServer.Business.Dtos.Categories
{
    /// <summary>
    /// multipart/form-data: Image + Order + Translations/SubCategories (JSON string).
    /// </summary>
    public class CategoryFormRequest
    {
        public IFormFile? Image { get; set; }

        public int Order { get; set; }

        /// <summary>JSON: [{ "languageCode":"az", "name":"..." }, ...]</summary>
        public string Translations { get; set; } = "[]";

        /// <summary>JSON: create/update subcategory payload array</summary>
        public string SubCategories { get; set; } = "[]";
    }
}
