namespace ARCServer.Business.Dtos.Catalog
{
    public class CatalogFilterOptionDto
    {
        public int Id { get; set; }

        public string Label { get; set; } = string.Empty;

        public string? HexCode { get; set; }
    }

    public class CatalogFilterOptionsDto
    {
        public IReadOnlyList<CatalogFilterOptionDto> Brands { get; set; } = [];

        public IReadOnlyList<CatalogFilterOptionDto> Sizes { get; set; } = [];

        public IReadOnlyList<CatalogFilterOptionDto> Diameters { get; set; } = [];

        public IReadOnlyList<CatalogFilterOptionDto> Powers { get; set; } = [];

        public IReadOnlyList<CatalogFilterOptionDto> Countries { get; set; } = [];

        public IReadOnlyList<CatalogFilterOptionDto> Colors { get; set; } = [];
    }
}
