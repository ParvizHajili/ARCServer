using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class HeroVideoConfiguration : IEntityTypeConfiguration<HeroVideo>
    {
        public void Configure(EntityTypeBuilder<HeroVideo> builder)
        {
            builder.ToTable("HeroVideos");

            builder.Property(x => x.VideoUrl)
                .HasMaxLength(500)
                .IsRequired();
        }
    }
}
