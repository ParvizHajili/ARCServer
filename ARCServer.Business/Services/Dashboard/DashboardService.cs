using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Dashboard;
using ARCServer.Data.Repositories;
using ARCServer.Domain.Entities;
using ARCServer.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<SubCategory> _subCategoryRepository;
        private readonly IRepository<Brand> _brandRepository;
        private readonly IRepository<Color> _colorRepository;
        private readonly IRepository<ManufacturerCountry> _countryRepository;
        private readonly IRepository<ProductSpinImage> _spinRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(
            IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IRepository<SubCategory> subCategoryRepository,
            IRepository<Brand> brandRepository,
            IRepository<Color> colorRepository,
            IRepository<ManufacturerCountry> countryRepository,
            IRepository<ProductSpinImage> spinRepository,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _subCategoryRepository = subCategoryRepository;
            _brandRepository = brandRepository;
            _colorRepository = colorRepository;
            _countryRepository = countryRepository;
            _spinRepository = spinRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<DashboardOverviewDto>> GetOverviewAsync(
            CancellationToken cancellationToken = default)
        {
            var since = DateTime.UtcNow.AddDays(-30);
            var products = _productRepository.Query().AsNoTracking();

            var categoryCounts = await products
                .GroupBy(p => p.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var categories = await _categoryRepository.Query()
                .AsNoTracking()
                .Include(c => c.Translations)
                .ToListAsync(cancellationToken);

            var byCategory = categories
                .Select(c => new DashboardCategoryStatDto
                {
                    CategoryId = c.Id,
                    Name = AzName(c.Translations.Select(t => (t.LanguageCode, t.Name, t.Deleted))),
                    ProductCount = categoryCounts.FirstOrDefault(x => x.CategoryId == c.Id)?.Count ?? 0,
                })
                .OrderByDescending(c => c.ProductCount)
                .ThenBy(c => c.Name)
                .ToList();

            var top = await products
                .Where(p => p.ViewCount > 0)
                .OrderByDescending(p => p.ViewCount)
                .ThenBy(p => p.Code)
                .Take(8)
                .Select(p => new DashboardTopProductDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    ViewCount = p.ViewCount,
                    Name = p.Translations
                        .Where(t => t.Deleted == 0)
                        .OrderBy(t => t.LanguageCode == "az" ? 0 : 1)
                        .Select(t => t.Name)
                        .FirstOrDefault() ?? string.Empty,
                })
                .ToListAsync(cancellationToken);

            var overview = new DashboardOverviewDto
            {
                CategoryCount = categories.Count,
                SubCategoryCount = await _subCategoryRepository.Query().CountAsync(cancellationToken),
                ProductCount = await products.CountAsync(cancellationToken),
                UserCount = await _userManager.Users.CountAsync(u => u.IsActive, cancellationToken),
                BrandCount = await _brandRepository.Query().CountAsync(cancellationToken),
                ColorCount = await _colorRepository.Query().CountAsync(cancellationToken),
                ManufacturerCountryCount = await _countryRepository.Query().CountAsync(cancellationToken),
                WarrantyCount = await products.CountAsync(p => p.HasWarranty, cancellationToken),
                MadeToOrderCount = await products.CountAsync(p => p.IsMadeToOrder, cancellationToken),
                SpinProductCount = await _spinRepository.Query()
                    .Select(s => s.ProductId)
                    .Distinct()
                    .CountAsync(cancellationToken),
                TotalViews = await products.SumAsync(p => (int?)p.ViewCount, cancellationToken) ?? 0,
                AddedLast30Days = await products.CountAsync(p => p.CreateDate >= since, cancellationToken),
                ProductsByCategory = byCategory,
                TopProducts = top,
            };

            return ServiceResult<DashboardOverviewDto>.Success(overview);
        }

        public async Task<ServiceResult<bool>> RecordViewByCodeAsync(
            string code,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return ServiceResult<bool>.Failure(
                    "code",
                    ErrorMessages.Format(ErrorMessages.Common.NotEmpty, ErrorMessages.Fields.Code));
            }

            var normalized = code.Trim().ToLower();
            var product = await _productRepository.Query()
                .FirstOrDefaultAsync(p => p.Code.ToLower() == normalized, cancellationToken);
            if (product is null)
            {
                return ServiceResult<bool>.NotFound("code", ErrorMessages.Product.NotFound);
            }

            product.ViewCount += 1;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult<bool>.Success(true);
        }

        private static string AzName(IEnumerable<(string LanguageCode, string Name, int Deleted)> translations)
        {
            var list = translations.Where(t => t.Deleted == 0).ToList();
            var az = list.FirstOrDefault(t => t.LanguageCode == "az").Name;
            if (!string.IsNullOrWhiteSpace(az))
            {
                return az;
            }

            return list.Select(t => t.Name).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? "—";
        }
    }
}
