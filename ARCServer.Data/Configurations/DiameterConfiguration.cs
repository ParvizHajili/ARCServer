using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class DiameterConfiguration : IEntityTypeConfiguration<Diameter>
    {
        public void Configure(EntityTypeBuilder<Diameter> builder)
        {
            builder.ToTable("Diameters");

            builder.Property(x => x.Value)
                .HasPrecision(18, 3);

            builder.HasIndex(x => x.Value)
                .IsUnique()
                .HasFilter("[Deleted] = 0");
        }
    }
}
