using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class Product360VideoConfiguration : IEntityTypeConfiguration<Product360Video>
    {
        public void Configure(EntityTypeBuilder<Product360Video> builder)
        {
            builder.ToTable("Product360Videos");

            builder.Property(x => x.VideoUrl)
                .HasMaxLength(500)
                .IsRequired();

            builder.HasIndex(x => new { x.ProductId, x.Order });
        }
    }
}
