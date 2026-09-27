using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class ProductSizeConfiguration : IEntityTypeConfiguration<ProductSize>
    {
        public void Configure(EntityTypeBuilder<ProductSize> builder)
        {
            builder.ToTable("ProductSizes");

            builder.HasIndex(x => new { x.ProductId, x.SizeId })
                .IsUnique()
                .HasFilter("[Deleted] = 0");

            builder.HasOne(x => x.Size)
                .WithMany()
                .HasForeignKey(x => x.SizeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
