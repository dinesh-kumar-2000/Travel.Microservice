using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.HasIndex(r => r.Name).IsUnique();

        // Note: UserRole relationship is handled through UserRoleConfiguration
    }
}
