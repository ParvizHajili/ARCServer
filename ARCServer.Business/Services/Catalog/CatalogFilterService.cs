using ARCServer.Business.Common;
using ARCServer.Business.Common.Localization;
using ARCServer.Business.Dtos.Catalog;
using ARCServer.Data.Repositories;
using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Catalog
{
    public interface ICatalogFilterService
    {
        Task<ServiceResult<CatalogFilterOptionsDto>> GetOptionsAsync(
            string? lang,
            CancellationToken cancellationToken = default);
    }

    public sealed class CatalogFilterService : ICatalogFilterService
    {
        private readonly IRepository<Brand> _brandRepository;
        private readonly IRepository<Size> _sizeRepository;
        private readonly IRepository<Diameter> _diameterRepository;
        private readonly IRepository<Power> _powerRepository;
        private readonly IRepository<ManufacturerCountry> _countryRepository;
        private readonly IRepository<Color> _colorRepository;

        public CatalogFilterService(
            IRepository<Brand> brandRepository,
            IRepository<Size> sizeRepository,
            IRepository<Diameter> diameterRepository,
            IRepository<Power> powerRepository,
            IRepository<ManufacturerCountry> countryRepository,
            IRepository<Color> colorRepository)
        {
            _brandRepository = brandRepository;
            _sizeRepository = sizeRepository;
            _diameterRepository = diameterRepository;
            _powerRepository = powerRepository;
            _countryRepository = countryRepository;
            _colorRepository = colorRepository;
        }

        public async Task<ServiceResult<CatalogFilterOptionsDto>> GetOptionsAsync(
            string? lang,
            CancellationToken cancellationToken = default)
        {
            var language = TranslationExtensions.NormalizeLanguageCode(lang);

            // Project only fields needed for filter labels — no full entity graphs.
            var brandRows = await _brandRepository.Query()
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    Translations = x.Translations
                        .Where(t => t.Deleted == 0)
                        .Select(t => new { t.LanguageCode, t.Name })
                        .ToList(),
                })
                .ToListAsync(cancellationToken);

            var sizeRows = await _sizeRepository.Query()
                .AsNoTracking()
                .OrderBy(x => x.Value)
                .Select(x => new { x.Id, x.Value })
                .ToListAsync(cancellationToken);

            var diameterRows = await _diameterRepository.Query()
                .AsNoTracking()
                .OrderBy(x => x.Value)
                .Select(x => new { x.Id, x.Value })
                .ToListAsync(cancellationToken);

            var powerRows = await _powerRepository.Query()
                .AsNoTracking()
                .OrderBy(x => x.Value)
                .Select(x => new { x.Id, x.Value })
                .ToListAsync(cancellationToken);

            var countryRows = await _countryRepository.Query()
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    Translations = x.Translations
                        .Where(t => t.Deleted == 0)
                        .Select(t => new { t.LanguageCode, t.Name })
                        .ToList(),
                })
                .ToListAsync(cancellationToken);

            var colorRows = await _colorRepository.Query()
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.HexCode,
                    Translations = x.Translations
                        .Where(t => t.Deleted == 0)
                        .Select(t => new { t.LanguageCode, t.Name })
                        .ToList(),
                })
                .ToListAsync(cancellationToken);

            var options = new CatalogFilterOptionsDto
            {
                Brands = brandRows
                    .Select(x => new CatalogFilterOptionDto
                    {
                        Id = x.Id,
                        Label = x.Translations
                            .Select(t => (t.LanguageCode, t.Name))
                            .ResolveTranslatedName(language),
                    })
                    .Where(x => x.Label.Length > 0)
                    .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                Sizes = sizeRows
                    .Select(x => new CatalogFilterOptionDto
                    {
                        Id = x.Id,
                        Label = FormatDecimal(x.Value),
                    })
                    .ToList(),
                Diameters = diameterRows
                    .Select(x => new CatalogFilterOptionDto
                    {
                        Id = x.Id,
                        Label = FormatDecimal(x.Value),
                    })
                    .ToList(),
                Powers = powerRows
                    .Select(x => new CatalogFilterOptionDto
                    {
                        Id = x.Id,
                        Label = FormatDecimal(x.Value),
                    })
                    .ToList(),
                Countries = countryRows
                    .Select(x => new CatalogFilterOptionDto
                    {
                        Id = x.Id,
                        Label = x.Translations
                            .Select(t => (t.LanguageCode, t.Name))
                            .ResolveTranslatedName(language),
                    })
                    .Where(x => x.Label.Length > 0)
                    .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                Colors = colorRows
                    .Select(x => new CatalogFilterOptionDto
                    {
                        Id = x.Id,
                        Label = x.Translations
                            .Select(t => (t.LanguageCode, t.Name))
                            .ResolveTranslatedName(language),
                        HexCode = x.HexCode,
                    })
                    .Where(x => x.Label.Length > 0)
                    .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
            };

            return ServiceResult<CatalogFilterOptionsDto>.Success(options);
        }

        private static string FormatDecimal(decimal value) =>
            value.ToString("0.##");
    }
}
