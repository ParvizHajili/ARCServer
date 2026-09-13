using System.Text.Json;
using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;

namespace ARCServer.Business.Dtos.Categories
{
    public static class CategoryFormMapper
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public static ServiceResult<CreateCategoryDto> ToCreateDto(CategoryFormRequest form)
        {
            if (!TryDeserialize(form.Translations, out List<TranslationInputDto>? translations, out var translationsError))
            {
                return ServiceResult<CreateCategoryDto>.Failure("translations", translationsError!);
            }

            if (!TryDeserialize(form.SubCategories, out List<CreateSubCategoryDto>? subCategories, out var subError))
            {
                return ServiceResult<CreateCategoryDto>.Failure("subCategories", subError!);
            }

            return ServiceResult<CreateCategoryDto>.Success(new CreateCategoryDto
            {
                Image = form.Image,
                Order = form.Order,
                Translations = translations ?? [],
                SubCategories = subCategories ?? [],
            });
        }

        public static ServiceResult<UpdateCategoryDto> ToUpdateDto(CategoryFormRequest form)
        {
            if (!TryDeserialize(form.Translations, out List<TranslationInputDto>? translations, out var translationsError))
            {
                return ServiceResult<UpdateCategoryDto>.Failure("translations", translationsError!);
            }

            if (!TryDeserialize(form.SubCategories, out List<UpdateSubCategoryDto>? subCategories, out var subError))
            {
                return ServiceResult<UpdateCategoryDto>.Failure("subCategories", subError!);
            }

            return ServiceResult<UpdateCategoryDto>.Success(new UpdateCategoryDto
            {
                Image = form.Image,
                Order = form.Order,
                Translations = translations ?? [],
                SubCategories = subCategories ?? [],
            });
        }

        private static bool TryDeserialize<T>(
            string? json,
            out T? value,
            out string? error)
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
                error = typeof(T).Name.Contains("SubCategory", StringComparison.Ordinal)
                    ? ErrorMessages.Category.InvalidSubCategoriesJson
                    : ErrorMessages.Category.InvalidTranslationsJson;
                return false;
            }
        }
    }
}
