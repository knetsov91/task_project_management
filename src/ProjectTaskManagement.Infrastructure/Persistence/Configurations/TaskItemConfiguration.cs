using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTaskManagement.Core.Entities;

namespace ProjectTaskManagement.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks", table =>
        {
            table.HasCheckConstraint("CK_Tasks_Status", "[Status] IN ('Todo', 'InProgress', 'Completed')");
            table.HasCheckConstraint("CK_Tasks_Priority", "[Priority] IN ('Low', 'Medium', 'High')");
        });
        builder.HasKey(task => task.Id);
        builder.Property(task => task.Id).ValueGeneratedNever();

        builder.Property(task => task.Title)
            .HasMaxLength(TaskItem.MaxTitleLength)
            .IsRequired();

        builder.Property(task => task.Description)
            .HasMaxLength(TaskItem.MaxDescriptionLength);

        builder.Property(task => task.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(task => task.Priority)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(task => task.DueDate).HasColumnType("datetimeoffset");
        builder.Property(task => task.CreatedAt)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(task => task.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(task => task.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(task => task.ProjectId);
        builder.HasIndex(task => task.AssignedToId);
    }
}
