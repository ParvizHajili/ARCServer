namespace ARCServer.Business.Common.Messages
{
    /// <summary>
    /// Dashboard API xəta mesajları (Azərbaycan dilində).
    /// </summary>
    public static class ErrorMessages
    {
        public static class Common
        {
            public const string Required = "{0} mütləqdir.";
            public const string NotEmpty = "{0} boş ola bilməz.";
            public const string MaxLength = "{0} maksimum {1} simvol ola bilər.";
            public const string GreaterThan = "{0} {1}-dən böyük olmalıdır.";
            public const string NotFound = "{0} tapılmadı.";
            public const string InvalidValue = "{0} düzgün deyil.";
        }

        public static class Fields
        {
            public const string Image = "Şəkil";
            public const string Order = "Sıra";
            public const string Name = "Ad";
            public const string LanguageCode = "Dil kodu";
            public const string Translations = "Tərcümələr";
            public const string SubCategories = "Alt kateqoriyalar";
            public const string Category = "Kateqoriya";
            public const string SubCategory = "Alt kateqoriya";
            public const string ManufacturerCountry = "İstehsalçı ölkə";
            public const string Brand = "Marka";
            public const string Color = "Rəng";
            public const string HexCode = "Rəng kodu";
            public const string Product = "Məhsul";
            public const string Code = "Kod";
            public const string Size = "Ölçü";
            public const string Diameter = "Diametr";
            public const string PowerAmperes = "Güc (amper)";
            public const string Description = "Təsvir";
            public const string Brands = "Markalar";
            public const string ManufacturerCountries = "İstehsalçı ölkələr";
            public const string Colors = "Rənglər";
            public const string Id = "Id";
        }

        public static class Category
        {
            public const string OrderExists = "'{0}' sıra nömrəsi artıq mövcuddur.";
            public const string NotFound = "Kateqoriya tapılmadı.";
            public const string SubCategoryNotFound = "Alt kateqoriya bu kateqoriyaya aid deyil və ya tapılmadı.";
            public const string TranslationsRequired =
                "Azərbaycan dili tərcüməsi mütləqdir.";
            public const string TranslationsUnique = "Kateqoriya tərcümələrində dil kodları unikaldır olmalıdır.";
            public const string SubTranslationsRequired =
                "Alt kateqoriya üçün Azərbaycan dili tərcüməsi mütləqdir.";
            public const string SubTranslationsUnique = "Alt kateqoriya tərcümələrində dil kodları unikaldır olmalıdır.";
            public const string InvalidLanguageCode = "Dil kodu düzgün deyil. İcazə verilən dillər: az, en, ru.";
            public const string SubCategoryIdsUnique = "Alt kateqoriya id-ləri unikaldır olmalıdır.";
            public const string InvalidTranslationsJson = "Tərcümələr JSON formatı düzgün deyil.";
            public const string InvalidSubCategoriesJson = "Alt kateqoriyalar JSON formatı düzgün deyil.";
            public const string ImageUploadFailed = "Şəkil yüklənərkən xəta baş verdi.";
        }

        public static class ManufacturerCountry
        {
            public const string NotFound = "İstehsalçı ölkə tapılmadı.";
            public const string TranslationsRequired =
                "Azərbaycan dili tərcüməsi mütləqdir.";
            public const string TranslationsUnique =
                "Ölkə tərcümələrində dil kodları unikaldır olmalıdır.";
            public const string NameExists = "'{0}' adı artıq mövcuddur.";
        }

        public static class Brand
        {
            public const string NotFound = "Marka tapılmadı.";
            public const string TranslationsRequired =
                "Azərbaycan dili tərcüməsi mütləqdir.";
            public const string TranslationsUnique =
                "Marka tərcümələrində dil kodları unikaldır olmalıdır.";
            public const string NameExists = "'{0}' adı artıq mövcuddur.";
        }

        public static class Color
        {
            public const string NotFound = "Rəng tapılmadı.";
            public const string TranslationsRequired =
                "Azərbaycan dili tərcüməsi mütləqdir.";
            public const string TranslationsUnique =
                "Rəng tərcümələrində dil kodları unikaldır olmalıdır.";
            public const string NameExists = "'{0}' adı artıq mövcuddur.";
            public const string HexExists = "'{0}' rəng kodu artıq mövcuddur.";
            public const string InvalidHex =
                "Rəng kodu #RRGGBB formatında olmalıdır (məs: #FF5733).";
        }

        public static class Product
        {
            public const string NotFound = "Məhsul tapılmadı.";
            public const string TranslationsRequired =
                "Azərbaycan dili tərcüməsi mütləqdir.";
            public const string TranslationsUnique =
                "Məhsul tərcümələrində dil kodları unikaldır olmalıdır.";
            public const string CodeExists = "'{0}' kodu artıq mövcuddur.";
            public const string CategoryNotFound = "Kateqoriya tapılmadı.";
            public const string SubCategoryInvalid =
                "Alt kateqoriya seçilmiş kateqoriyaya aid deyil və ya tapılmadı.";
            public const string BrandNotFound = "Seçilmiş markalardan biri tapılmadı.";
            public const string ManufacturerCountryNotFound =
                "Seçilmiş istehsalçı ölkələrdən biri tapılmadı.";
            public const string ColorNotFound = "Seçilmiş rənglərdən biri tapılmadı.";
            public const string BrandsRequired = "Ən azı bir marka seçilməlidir.";
            public const string ManufacturerCountriesRequired =
                "Ən azı bir istehsalçı ölkə seçilməlidir.";
            public const string ColorsRequired = "Ən azı bir rəng seçilməlidir.";
            public const string ColorImagesRequired =
                "Hər seçilmiş rəng üçün ən azı bir şəkil lazımdır.";
            public const string ImageColorIdsMismatch =
                "Şəkillər və rəng id-ləri uyğun gəlmir.";
            public const string InvalidJson = "JSON formatı düzgün deyil.";
            public const string ImageUploadFailed = "Şəkil yüklənərkən xəta baş verdi.";
            public const string IdsUnique = "{0} unikaldır olmalıdır.";
        }

        public static string Format(string template, params object[] args) =>
            string.Format(template, args);
    }
}
