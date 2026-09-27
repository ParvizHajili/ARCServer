namespace ARCServer.Business.Dtos.Dashboard
{
    public class DashboardCategoryStatDto
    {
        public int CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int ProductCount { get; set; }
    }

    public class DashboardTopProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public int ViewCount { get; set; }
    }

    public class DashboardOverviewDto
    {
        public int CategoryCount { get; set; }

        public int SubCategoryCount { get; set; }

        public int ProductCount { get; set; }

        public int UserCount { get; set; }

        public int BrandCount { get; set; }

        public int ColorCount { get; set; }

        public int ManufacturerCountryCount { get; set; }

        public int WarrantyCount { get; set; }

        public int MadeToOrderCount { get; set; }

        public int SpinProductCount { get; set; }

        public int TotalViews { get; set; }

        public int AddedLast30Days { get; set; }

        public List<DashboardCategoryStatDto> ProductsByCategory { get; set; } = [];

        public List<DashboardTopProductDto> TopProducts { get; set; } = [];
    }
}
