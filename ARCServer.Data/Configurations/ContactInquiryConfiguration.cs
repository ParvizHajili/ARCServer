using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ARCServer.Data.Configurations
{
    public class ContactInquiryConfiguration : IEntityTypeConfiguration<ContactInquiry>
    {
        public void Configure(EntityTypeBuilder<ContactInquiry> builder)
        {
            builder.ToTable("ContactInquiries");

            builder.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Phone)
                .HasMaxLength(30);

            builder.Property(x => x.Email)
                .HasMaxLength(200);

            builder.Property(x => x.Message)
                .HasMaxLength(2000)
                .IsRequired();

            builder.HasIndex(x => x.CreateDate);
        }
    }
}
