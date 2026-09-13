using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class ManufacturerCountryConfiguration : IEntityTypeConfiguration<ManufacturerCountry>
    {
        public void Configure(EntityTypeBuilder<ManufacturerCountry> builder)
        {
            builder.ToTable("ManufacturerCountries");

            builder.HasMany(x => x.Translations)
                .WithOne(x => x.ManufacturerCountry)
                .HasForeignKey(x => x.ManufacturerCountryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
