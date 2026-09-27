using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class SizeConfiguration : IEntityTypeConfiguration<Size>
    {
        public void Configure(EntityTypeBuilder<Size> builder)
        {
            builder.ToTable("Sizes");

            builder.Property(x => x.Value)
                .HasPrecision(18, 3);

            builder.HasIndex(x => x.Value)
                .IsUnique()
                .HasFilter("[Deleted] = 0");
        }
    }
}
