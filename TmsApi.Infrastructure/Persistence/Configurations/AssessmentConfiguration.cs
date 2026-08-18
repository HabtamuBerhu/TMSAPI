using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        // 1. Primary Key
        builder.HasKey(a => a.Id);

        // 2. Property Limits
        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(150);

        // Configure precision for decimal values (total digits, decimal places)
        // e.g., MaxScore can be up to 999.99, Weight can store up to 0.1234 (12.34%)
        builder.Property(a => a.MaxScore)
            .HasPrecision(5, 2); 

        builder.Property(a => a.Weight)
            .HasPrecision(4, 2); 

        // 3. Relationships
        // Links back to the owning Course and uses Restrict behavior
        builder.HasOne(a => a.Course)
            .WithMany() // Adjust if Course has an 'Assessments' collection
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}