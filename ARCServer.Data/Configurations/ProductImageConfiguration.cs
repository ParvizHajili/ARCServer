using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");

            builder.Property(x => x.ImageUrl)
                .HasMaxLength(500)
                .IsRequired();

            // First gallery image lookup for catalog cards
            builder.HasIndex(x => new { x.ProductId, x.ProductColorId, x.Order });
        }
    }
}
