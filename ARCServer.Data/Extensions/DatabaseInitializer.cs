using ARCServer.Data.Context;
using ARCServer.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ARCServer.Data.Extensions
{
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Startup DB check:
        /// - no DB → create + apply all migrations
        /// - DB exists, migrations up to date → no-op for migrations
        /// - DB exists, pending migrations → apply update
        /// Then seed default categories if the table is empty.
        /// </summary>
        public static async Task InitializeArcDatabaseAsync(
            this IServiceProvider services,
            CancellationToken cancellationToken = default)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ArcDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("ARCServer.Database");

            var connectionString = db.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'ApplicationDb' is missing or empty.");
            }

            try
            {
                var canConnect = await db.Database.CanConnectAsync(cancellationToken);

                if (!canConnect)
                {
                    logger.LogInformation(
                        "Database not found. Creating database and applying migrations...");
                    await db.Database.MigrateAsync(cancellationToken);
                    logger.LogInformation("Database created and migrations applied successfully.");
                }
                else
                {
                    var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
                    if (pending.Count == 0)
                    {
                        logger.LogInformation(
                            "Database already exists and is up to date. No migration action needed.");
                    }
                    else
                    {
                        logger.LogInformation(
                            "Database is outdated. Applying {Count} pending migration(s): {Migrations}",
                            pending.Count,
                            string.Join(", ", pending));

                        await db.Database.MigrateAsync(cancellationToken);

                        logger.LogInformation("Database updated to the latest migration successfully.");
                    }
                }

                await CategorySeeder.SeedAsync(db, logger, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogCritical(
                    ex,
                    "Database initialization failed. Verify SQL Server and ApplicationDb connection string.");
                throw;
            }
        }
    }
}
