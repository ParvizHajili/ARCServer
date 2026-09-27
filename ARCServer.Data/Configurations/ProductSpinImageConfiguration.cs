using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class ProductSpinImageConfiguration : IEntityTypeConfiguration<ProductSpinImage>
    {
        public void Configure(EntityTypeBuilder<ProductSpinImage> builder)
        {
            builder.ToTable("ProductSpinImages");

            builder.Property(x => x.ImageUrl)
                .HasMaxLength(500)
                .IsRequired();

            builder.HasIndex(x => new { x.ProductId, x.Order });
        }
    }
}
