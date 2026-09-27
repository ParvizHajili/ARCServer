using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class ProductDiameterConfiguration : IEntityTypeConfiguration<ProductDiameter>
    {
        public void Configure(EntityTypeBuilder<ProductDiameter> builder)
        {
            builder.ToTable("ProductDiameters");

            builder.HasIndex(x => new { x.ProductId, x.DiameterId })
                .IsUnique()
                .HasFilter("[Deleted] = 0");

            builder.HasOne(x => x.Diameter)
                .WithMany()
                .HasForeignKey(x => x.DiameterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
