using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class ProductManufacturerCountryConfiguration
        : IEntityTypeConfiguration<ProductManufacturerCountry>
    {
        public void Configure(EntityTypeBuilder<ProductManufacturerCountry> builder)
        {
            builder.ToTable("ProductManufacturerCountries");

            builder.HasIndex(x => new { x.ProductId, x.ManufacturerCountryId })
                .IsUnique()
                .HasFilter("[Deleted] = 0");

            builder.HasOne(x => x.ManufacturerCountry)
                .WithMany()
                .HasForeignKey(x => x.ManufacturerCountryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
