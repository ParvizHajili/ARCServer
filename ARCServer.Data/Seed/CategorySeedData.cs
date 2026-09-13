using ARCServer.Domain.Entities;

namespace ARCServer.Data.Seed
{
    /// <summary>
    /// Default dashboard categories (lighting catalog sample).
    /// Images are Cloudinary URLs already uploaded for ARC.
    /// </summary>
    public static class CategorySeedData
    {
        public static IReadOnlyList<Category> Build(DateTime now)
        {
            return
            [
                BuildCategory(
                    now,
                    order: 1,
                    image: "https://res.cloudinary.com/dyiqsbyb7/image/upload/v1789319465/category/qy1nmlvog7xo7qhsfhcl.jpg",
                    translations:
                    [
                        ("az", "LED Panellər"),
                        ("en", "LED Panels"),
                        ("ru", "LED панели"),
                    ],
                    subCategories:
                    [
                        [("az", "Kvadrat panellər"), ("en", "Square panels"), ("ru", "Квадратные панели")],
                        [("az", "Dairəvi panellər"), ("en", "Round panels"), ("ru", "Круглые панели")],
                        [("az", "İncə panellər"), ("en", "Slim panels"), ("ru", "Тонкие панели")],
                    ]),
                BuildCategory(
                    now,
                    order: 2,
                    image: "https://res.cloudinary.com/dyiqsbyb7/image/upload/v1789319467/category/be0hyfjto0pmumumnxyk.jpg",
                    translations:
                    [
                        ("az", "Spot İşıqlar"),
                        ("en", "Spot Lights"),
                        ("ru", "Точечные светильники"),
                    ],
                    subCategories:
                    [
                        [("az", "Tavan spotları"), ("en", "Ceiling spots"), ("ru", "Потолочные споты")],
                        [("az", "Raylı sistemlər"), ("en", "Track systems"), ("ru", "Трековые системы")],
                    ]),
                BuildCategory(
                    now,
                    order: 3,
                    image: "https://res.cloudinary.com/dyiqsbyb7/image/upload/v1789319468/category/sbwsxprrb9fprx0bbpu5.jpg",
                    translations:
                    [
                        ("az", "Çilçıraqlar"),
                        ("en", "Chandeliers"),
                        ("ru", "Люстры"),
                    ],
                    subCategories:
                    [
                        [("az", "Klassik"), ("en", "Classic"), ("ru", "Классика")],
                        [("az", "Modern"), ("en", "Modern"), ("ru", "Модерн")],
                        [("az", "Kristal"), ("en", "Crystal"), ("ru", "Хрусталь")],
                        [("az", "Minimal"), ("en", "Minimal"), ("ru", "Минимализм")],
                    ]),
                BuildCategory(
                    now,
                    order: 4,
                    image: "https://res.cloudinary.com/dyiqsbyb7/image/upload/v1789319469/category/tiygpc60dx9ppklipdtp.jpg",
                    translations:
                    [
                        ("az", "Divar İşıqları"),
                        ("en", "Wall Lights"),
                        ("ru", "Настенные светильники"),
                    ],
                    subCategories:
                    [
                        [("az", "Bra"), ("en", "Wall sconces"), ("ru", "Бра")],
                        [("az", "LED lentlər"), ("en", "LED strips"), ("ru", "LED ленты")],
                        [("az", "Up-down"), ("en", "Up-down lights"), ("ru", "Up-down светильники")],
                        [("az", "Güzgü işıqları"), ("en", "Mirror lights"), ("ru", "Зеркальные светильники")],
                        [("az", "Dekor bra"), ("en", "Decorative sconces"), ("ru", "Декоративные бра")],
                    ]),
                BuildCategory(
                    now,
                    order: 5,
                    image: "https://res.cloudinary.com/dyiqsbyb7/image/upload/v1789319471/category/zfnldhopovcnkr0s30sj.jpg",
                    translations:
                    [
                        ("az", "Bağ İşıqları"),
                        ("en", "Garden Lights"),
                        ("ru", "Садовые светильники"),
                    ],
                    subCategories: []),
                BuildCategory(
                    now,
                    order: 6,
                    image: "https://res.cloudinary.com/dyiqsbyb7/image/upload/v1789319552/category/yrbmtqotdiao6sj6eau9.jpg",
                    translations:
                    [
                        ("az", "Ofis İşıqları"),
                        ("en", "Office Lighting"),
                    ],
                    subCategories:
                    [
                        [("az", "Asma ofis panelləri"), ("en", "Suspended office panels")],
                        [("az", "Masa lampaları"), ("en", "Desk lamps")],
                        [("az", "Linear ofis işıqları"), ("en", "Linear office lights")],
                    ]),
                BuildCategory(
                    now,
                    order: 7,
                    image: "https://res.cloudinary.com/dyiqsbyb7/image/upload/v1789319553/category/vmniwo9pblpvr1rzffzs.jpg",
                    translations:
                    [
                        ("az", "Sənaye İşıqları"),
                        ("ru", "Промышленное освещение"),
                    ],
                    subCategories:
                    [
                        [("az", "High-bay"), ("ru", "High-bay светильники")],
                        [("az", "Anbar işıqları"), ("ru", "Складские светильники")],
                    ]),
                BuildCategory(
                    now,
                    order: 8,
                    image: "https://res.cloudinary.com/dyiqsbyb7/image/upload/v1789319554/category/ex7ekisnr5vr84llnhjl.jpg",
                    translations:
                    [
                        ("az", "Dekorativ İşıqlar"),
                    ],
                    subCategories: []),
            ];
        }

        private static Category BuildCategory(
            DateTime now,
            int order,
            string image,
            (string Code, string Name)[] translations,
            (string Code, string Name)[][] subCategories)
        {
            var category = new Category
            {
                Image = image,
                Order = order,
                CreateDate = now,
                Deleted = 0,
            };

            foreach (var (code, name) in translations)
            {
                category.Translations.Add(new CategoryTranslation
                {
                    LanguageCode = code,
                    Name = name,
                    CreateDate = now,
                    Deleted = 0,
                });
            }

            foreach (var subTranslations in subCategories)
            {
                var sub = new SubCategory
                {
                    CreateDate = now,
                    Deleted = 0,
                };

                foreach (var (code, name) in subTranslations)
                {
                    sub.Translations.Add(new SubCategoryTranslation
                    {
                        LanguageCode = code,
                        Name = name,
                        CreateDate = now,
                        Deleted = 0,
                    });
                }

                category.SubCategories.Add(sub);
            }

            return category;
        }
    }
}
