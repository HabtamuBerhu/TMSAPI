using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Shadow property for Exercise 8
        builder.Property<DateTime>("LastUpdated");

        // Concurrency token for Exercise 8
        builder.Property(s => s.Version)
            .IsRowVersion();
        // One-to-Many: Student has many Enrollments
        builder.HasMany(s => s.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
            // Fails loudly if trying to delete a student with enrollments
        // Soft delete filter for Exercise 9
        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasMany(s => s.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}