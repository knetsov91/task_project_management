using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTaskManagement.Core.Entities;

namespace ProjectTaskManagement.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public const string OwnerIdIndexName = "IX_Projects_OwnerId";

    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(project => project.Id);

        builder.Property(project => project.Id).ValueGeneratedNever();

        builder.Property(project => project.Name)
            .HasMaxLength(Project.MaxNameLength)
            .IsRequired();

        builder.Property(project => project.Description)
            .HasMaxLength(Project.MaxDescriptionLength);

        builder.Property(project => project.OwnerId)
            .IsRequired();

        builder.Property(project => project.CreatedAt)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(project => project.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(project => project.OwnerId)
            .HasDatabaseName(OwnerIdIndexName);
    }
}
