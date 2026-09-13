using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class ColorConfiguration : IEntityTypeConfiguration<Color>
    {
        public void Configure(EntityTypeBuilder<Color> builder)
        {
            builder.ToTable("Colors");

            builder.Property(x => x.HexCode)
                .HasMaxLength(7)
                .IsRequired();

            builder.HasIndex(x => x.HexCode)
                .IsUnique()
                .HasFilter("[Deleted] = 0");

            builder.HasMany(x => x.Translations)
                .WithOne(x => x.Color)
                .HasForeignKey(x => x.ColorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
