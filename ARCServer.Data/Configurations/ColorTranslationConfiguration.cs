using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class ColorTranslationConfiguration : IEntityTypeConfiguration<ColorTranslation>
    {
        public void Configure(EntityTypeBuilder<ColorTranslation> builder)
        {
            builder.ToTable("ColorTranslations");

            builder.Property(x => x.LanguageCode)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(x => new { x.ColorId, x.LanguageCode })
                .IsUnique()
                .HasFilter("[Deleted] = 0");

            builder.HasIndex(x => new { x.LanguageCode, x.Name })
                .IsUnique()
                .HasFilter("[Deleted] = 0");
        }
    }
}
