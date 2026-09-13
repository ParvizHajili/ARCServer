using ARCServer.Business.Services.Brands;
using ARCServer.Business.Services.Categories;
using ARCServer.Business.Services.Colors;
using ARCServer.Business.Services.ManufacturerCountries;
using ARCServer.Business.Services.Products;
using ARCServer.Business.Services.Storage;
using ARCServer.Business.Settings;
using ARCServer.Business.Validators.Categories;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ARCServer.Business.Extensions
{
    public static class BusinessServiceCollectionExtensions
    {
        public static IServiceCollection AddArcBusiness(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<CloudinarySettings>(
                configuration.GetSection(CloudinarySettings.SectionName));

            services.AddSingleton<ICloudinaryService, CloudinaryService>();

            services.AddAutoMapper(typeof(BusinessServiceCollectionExtensions).Assembly);
            services.AddValidatorsFromAssemblyContaining<CreateCategoryDtoValidator>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IManufacturerCountryService, ManufacturerCountryService>();
            services.AddScoped<IBrandService, BrandService>();
            services.AddScoped<IColorService, ColorService>();
            services.AddScoped<IProductService, ProductService>();

            return services;
        }
    }
}
