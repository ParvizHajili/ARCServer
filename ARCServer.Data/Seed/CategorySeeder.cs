using ARCServer.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ARCServer.Data.Seed
{
    public static class CategorySeeder
    {
        /// <summary>
        /// Seeds default categories only when the Categories table has no active rows.
        /// Safe to call on every startup after migrations.
        /// </summary>
        public static async Task SeedAsync(
            ArcDbContext db,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            var hasCategories = await db.Categories
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Deleted == 0, cancellationToken);

            if (hasCategories)
            {
                logger.LogInformation("Category seed skipped — active categories already exist.");
                return;
            }

            var now = DateTime.UtcNow;
            var categories = CategorySeedData.Build(now);

            await db.Categories.AddRangeAsync(categories, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Category seed completed. Inserted {Count} categories.",
                categories.Count);
        }
    }
}
