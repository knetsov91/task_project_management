using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTaskManagement.Core.Entities;

namespace ProjectTaskManagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public const string NormalizedEmailIndexName = "IX_Users_NormalizedEmail";

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id).ValueGeneratedNever();

        builder.Property(user => user.Name)
            .HasMaxLength(User.MaxNameLength)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(User.MaxEmailLength)
            .IsRequired();

        builder.Property(user => user.NormalizedEmail)
            .HasMaxLength(User.MaxEmailLength)
            .UseCollation("Latin1_General_100_BIN2")
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.HasIndex(user => user.NormalizedEmail)
            .HasDatabaseName(NormalizedEmailIndexName)
            .IsUnique();
    }
}
