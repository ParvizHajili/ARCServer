using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class ProductPowerConfiguration : IEntityTypeConfiguration<ProductPower>
    {
        public void Configure(EntityTypeBuilder<ProductPower> builder)
        {
            builder.ToTable("ProductPowers");

            builder.HasIndex(x => new { x.ProductId, x.PowerId })
                .IsUnique()
                .HasFilter("[Deleted] = 0");

            builder.HasOne(x => x.Power)
                .WithMany()
                .HasForeignKey(x => x.PowerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
