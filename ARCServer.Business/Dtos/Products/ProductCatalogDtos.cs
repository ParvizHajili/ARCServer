namespace ARCServer.Business.Dtos.Products
{
    public class ProductCatalogQueryDto
    {
        public int? CategoryId { get; set; }

        /// <summary>Comma-separated or single id list from query string.</summary>
        public string? SubCategoryIds { get; set; }

        public string? BrandIds { get; set; }

        public string? SizeIds { get; set; }

        public string? DiameterIds { get; set; }

        public string? PowerIds { get; set; }

        public string? CountryIds { get; set; }

        public string? ColorIds { get; set; }

        public bool? HasWarranty { get; set; }

        public bool? IsMadeToOrder { get; set; }

        public string? Search { get; set; }

        public string? Lang { get; set; } = "az";

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 12;

        public string NormalizedLang =>
            ARCServer.Business.Common.Localization.TranslationExtensions.NormalizeLanguageCode(Lang);

        public int SafePage => Page < 1 ? 1 : Page;

        /// <summary>Storefront page size is capped lower than dashboard MaxPageSize.</summary>
        public int SafePageSize =>
            PageSize < 1 ? 12 : Math.Min(PageSize, 48);

        public int Skip => (SafePage - 1) * SafePageSize;

        public IReadOnlyList<int> ParseIds(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return [];
            }

            var ids = new List<int>();
            foreach (var part in raw.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (int.TryParse(part, out var id) && id > 0 && !ids.Contains(id))
                {
                    ids.Add(id);
                }
            }

            return ids;
        }
    }

    public class PublicProductCardDto
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }

        public int? SubCategoryId { get; set; }

        public bool HasWarranty { get; set; }

        public bool IsMadeToOrder { get; set; }
    }
}
