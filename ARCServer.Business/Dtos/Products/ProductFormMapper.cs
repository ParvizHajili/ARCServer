using System.Text.Json;
using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;

namespace ARCServer.Business.Dtos.Products
{
    public static class ProductFormMapper
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public static ServiceResult<CreateProductDto> ToCreateDto(ProductFormRequest form)
        {
            if (!TryMapCommon(form, out var common, out var failure))
            {
                return ServiceResult<CreateProductDto>.Failure(
                    failure!.Errors.Keys.First(),
                    failure.Errors.Values.First().First());
            }

            return ServiceResult<CreateProductDto>.Success(common!);
        }

        public static ServiceResult<UpdateProductDto> ToUpdateDto(ProductFormRequest form)
        {
            if (!TryMapCommon(form, out var common, out var failure))
            {
                return ServiceResult<UpdateProductDto>.Failure(
                    failure!.Errors.Keys.First(),
                    failure.Errors.Values.First().First());
            }

            if (!TryDeserialize(form.KeepImageIds, out List<int>? keepImageIds, out _))
            {
                return ServiceResult<UpdateProductDto>.Failure(
                    "keepImageIds",
                    ErrorMessages.Product.InvalidJson);
            }

            if (!TryDeserialize(form.KeepProductImageIds, out List<int>? keepProductImageIds, out _))
            {
                return ServiceResult<UpdateProductDto>.Failure(
                    "keepProductImageIds",
                    ErrorMessages.Product.InvalidJson);
            }

            var update = new UpdateProductDto
            {
                Code = common!.Code,
                HasWarranty = common.HasWarranty,
                IsMadeToOrder = common.IsMadeToOrder,
                CategoryId = common.CategoryId,
                SubCategoryId = common.SubCategoryId,
                Translations = common.Translations,
                BrandIds = common.BrandIds,
                SizeIds = common.SizeIds,
                DiameterIds = common.DiameterIds,
                PowerIds = common.PowerIds,
                ManufacturerCountryIds = common.ManufacturerCountryIds,
                ColorIds = common.ColorIds,
                Images = common.Images,
                ImageColorIds = common.ImageColorIds,
                ProductImages = common.ProductImages,
                KeepImageIds = keepImageIds ?? [],
                KeepProductImageIds = keepProductImageIds ?? [],
            };

            return ServiceResult<UpdateProductDto>.Success(update);
        }

        private static bool TryMapCommon(
            ProductFormRequest form,
            out CreateProductDto? dto,
            out ServiceResult<CreateProductDto>? failure)
        {
            dto = null;
            failure = null;

            if (!TryDeserialize(form.Translations, out List<ProductTranslationInputDto>? translations, out _))
            {
                failure = ServiceResult<CreateProductDto>.Failure(
                    "translations",
                    ErrorMessages.Product.InvalidJson);
                return false;
            }

            if (!TryDeserialize(form.BrandIds, out List<int>? brandIds, out _))
            {
                failure = ServiceResult<CreateProductDto>.Failure(
                    "brandIds",
                    ErrorMessages.Product.InvalidJson);
                return false;
            }

            if (!TryDeserialize(form.SizeIds, out List<int>? sizeIds, out _))
            {
                failure = ServiceResult<CreateProductDto>.Failure(
                    "sizeIds",
                    ErrorMessages.Product.InvalidJson);
                return false;
            }

            if (!TryDeserialize(form.DiameterIds, out List<int>? diameterIds, out _))
            {
                failure = ServiceResult<CreateProductDto>.Failure(
                    "diameterIds",
                    ErrorMessages.Product.InvalidJson);
                return false;
            }

            if (!TryDeserialize(form.PowerIds, out List<int>? powerIds, out _))
            {
                failure = ServiceResult<CreateProductDto>.Failure(
                    "powerIds",
                    ErrorMessages.Product.InvalidJson);
                return false;
            }

            if (!TryDeserialize(form.ManufacturerCountryIds, out List<int>? countryIds, out _))
            {
                failure = ServiceResult<CreateProductDto>.Failure(
                    "manufacturerCountryIds",
                    ErrorMessages.Product.InvalidJson);
                return false;
            }

            if (!TryDeserialize(form.ColorIds, out List<int>? colorIds, out _))
            {
                failure = ServiceResult<CreateProductDto>.Failure(
                    "colorIds",
                    ErrorMessages.Product.InvalidJson);
                return false;
            }

            if (!TryDeserialize(form.ImageColorIds, out List<int>? imageColorIds, out _))
            {
                failure = ServiceResult<CreateProductDto>.Failure(
                    "imageColorIds",
                    ErrorMessages.Product.InvalidJson);
                return false;
            }

            var images = form.Images?
                .Where(f => f is { Length: > 0 })
                .ToList() ?? [];

            var productImages = form.ProductImages?
                .Where(f => f is { Length: > 0 })
                .ToList() ?? [];

            dto = new CreateProductDto
            {
                Code = form.Code?.Trim() ?? string.Empty,
                HasWarranty = form.HasWarranty,
                IsMadeToOrder = form.IsMadeToOrder,
                CategoryId = form.CategoryId,
                SubCategoryId = form.SubCategoryId is > 0 ? form.SubCategoryId : null,
                Translations = translations ?? [],
                BrandIds = brandIds ?? [],
                SizeIds = sizeIds ?? [],
                DiameterIds = diameterIds ?? [],
                PowerIds = powerIds ?? [],
                ManufacturerCountryIds = countryIds ?? [],
                ColorIds = colorIds ?? [],
                Images = images,
                ImageColorIds = imageColorIds ?? [],
                ProductImages = productImages,
            };

            return true;
        }

        private static bool TryDeserialize<T>(string? json, out T? value, out string? error)
        {
            value = default;
            error = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                value = JsonSerializer.Deserialize<T>("[]", JsonOptions);
                return true;
            }

            try
            {
                value = JsonSerializer.Deserialize<T>(json, JsonOptions);
                return true;
            }
            catch (JsonException)
            {
                error = ErrorMessages.Product.InvalidJson;
                return false;
            }
        }
    }
}
