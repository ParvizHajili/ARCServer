using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class PowerConfiguration : IEntityTypeConfiguration<Power>
    {
        public void Configure(EntityTypeBuilder<Power> builder)
        {
            builder.ToTable("Powers");

            builder.Property(x => x.Value)
                .HasPrecision(18, 3);

            builder.HasIndex(x => x.Value)
                .IsUnique()
                .HasFilter("[Deleted] = 0");
        }
    }
}
