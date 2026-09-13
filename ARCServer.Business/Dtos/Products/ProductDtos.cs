using Microsoft.AspNetCore.Http;

namespace ARCServer.Business.Dtos.Products
{
    public class ProductTranslationInputDto
    {
        public string LanguageCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    public class CreateProductDto
    {
        public string Code { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        public string Diameter { get; set; } = string.Empty;

        public bool HasWarranty { get; set; }

        public bool IsMadeToOrder { get; set; }

        public decimal PowerAmperes { get; set; }

        public int CategoryId { get; set; }

        public int? SubCategoryId { get; set; }

        public List<ProductTranslationInputDto> Translations { get; set; } = [];

        public List<int> BrandIds { get; set; } = [];

        public List<int> ManufacturerCountryIds { get; set; } = [];

        public List<int> ColorIds { get; set; } = [];

        public List<IFormFile> Images { get; set; } = [];

        public List<int> ImageColorIds { get; set; } = [];
    }

    public class UpdateProductDto : CreateProductDto
    {
        public List<int> KeepImageIds { get; set; } = [];
    }

    public class ProductTranslationResultDto
    {
        public string LanguageCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    public class ProductNamedRefDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public class ProductImageDetailDto
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int Order { get; set; }
    }

    public class ProductColorDetailDto
    {
        public int ColorId { get; set; }

        public string HexCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public List<ProductImageDetailDto> Images { get; set; } = [];
    }

    public class ProductDetailDto
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        public string Diameter { get; set; } = string.Empty;

        public bool HasWarranty { get; set; }

        public bool IsMadeToOrder { get; set; }

        public decimal PowerAmperes { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public int? SubCategoryId { get; set; }

        public string? SubCategoryName { get; set; }

        public List<ProductTranslationResultDto> Translations { get; set; } = [];

        public List<ProductNamedRefDto> Brands { get; set; } = [];

        public List<ProductNamedRefDto> ManufacturerCountries { get; set; } = [];

        public List<ProductColorDetailDto> Colors { get; set; } = [];
    }

    public class ProductListItemDto
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string? SubCategoryName { get; set; }

        public decimal PowerAmperes { get; set; }

        public bool HasWarranty { get; set; }

        public bool IsMadeToOrder { get; set; }
    }
}
