using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations; // Adjust to just TmsApi.Data if not in a subfolder

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(150);

        // One-to-Many: Course has many Enrollments
        builder.HasMany(c => c.Enrollments)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict); // Required by Exercise 5: Blocks course deletion if enrollments exist
    }
}