using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class ManufacturerCountryTranslationConfiguration
        : IEntityTypeConfiguration<ManufacturerCountryTranslation>
    {
        public void Configure(EntityTypeBuilder<ManufacturerCountryTranslation> builder)
        {
            builder.ToTable("ManufacturerCountryTranslations");

            builder.Property(x => x.LanguageCode)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(x => new { x.ManufacturerCountryId, x.LanguageCode })
                .IsUnique()
                .HasFilter("[Deleted] = 0");

            // Eyni dildə eyni ad təkrar oluna bilməz (aktiv sətirlər)
            builder.HasIndex(x => new { x.LanguageCode, x.Name })
                .IsUnique()
                .HasFilter("[Deleted] = 0");
        }
    }
}
