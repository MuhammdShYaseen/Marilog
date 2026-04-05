using Marilog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class NavItemConfiguration : IEntityTypeConfiguration<NavItem>
    {
        public void Configure(EntityTypeBuilder<NavItem> builder)
        {
            builder.ToTable("NavItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Route).HasMaxLength(200);
            builder.Property(x => x.Icon).HasMaxLength(50);

            builder.HasMany(typeof(NavItem), "_children")
                .WithOne()
                .HasForeignKey("ParentId");

            builder.Navigation(x => x.Children)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
