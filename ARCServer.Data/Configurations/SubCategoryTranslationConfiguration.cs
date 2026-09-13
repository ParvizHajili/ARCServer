using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class SubCategoryTranslationConfiguration : IEntityTypeConfiguration<SubCategoryTranslation>
    {
        public void Configure(EntityTypeBuilder<SubCategoryTranslation> builder)
        {
            builder.ToTable("SubCategoryTranslations");

            builder.Property(x => x.LanguageCode)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(x => new { x.SubCategoryId, x.LanguageCode })
                .IsUnique()
                .HasFilter("[Deleted] = 0");
        }
    }
}
