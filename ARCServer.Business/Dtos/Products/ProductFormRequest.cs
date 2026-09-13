using Microsoft.AspNetCore.Http;

namespace ARCServer.Business.Dtos.Products
{
    public class ProductFormRequest
    {
        public string Code { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        public string Diameter { get; set; } = string.Empty;

        public bool HasWarranty { get; set; }

        public bool IsMadeToOrder { get; set; }

        public decimal PowerAmperes { get; set; }

        public int CategoryId { get; set; }

        public int? SubCategoryId { get; set; }

        /// <summary>JSON: [{ languageCode, name, description }]</summary>
        public string Translations { get; set; } = "[]";

        /// <summary>JSON: int[]</summary>
        public string BrandIds { get; set; } = "[]";

        /// <summary>JSON: int[]</summary>
        public string ManufacturerCountryIds { get; set; } = "[]";

        /// <summary>JSON: int[]</summary>
        public string ColorIds { get; set; } = "[]";

        public List<IFormFile> Images { get; set; } = [];

        /// <summary>JSON: int[] — Images ilə eyni uzunluq</summary>
        public string ImageColorIds { get; set; } = "[]";

        /// <summary>JSON: int[] — yalnız update</summary>
        public string KeepImageIds { get; set; } = "[]";
    }
}
