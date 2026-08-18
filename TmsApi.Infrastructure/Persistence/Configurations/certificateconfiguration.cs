using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        // 1. Primary Key
        builder.HasKey(c => c.Id);

        // 2. Property Limits & Uniqueness Constraint for SerialNumber
        builder.Property(c => c.SerialNumber)
            .IsRequired()
            .HasMaxLength(50);

        // This enforces that no two certificates can have the same SerialNumber in PostgreSQL
        builder.HasIndex(c => c.SerialNumber)
            .IsUnique();

        // 3. Relationships
        builder.HasOne(c => c.Student)
            .WithMany() // Keeps it simple unless Student has a List<Certificate>
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Course)
            .WithMany() 
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}