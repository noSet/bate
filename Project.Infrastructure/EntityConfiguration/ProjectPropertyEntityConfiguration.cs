using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.AggregatesModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.EntityConfiguration
{
    class ProjectPropertyEntityConfiguration : IEntityTypeConfiguration<ProjectProperty>
    {
        public void Configure(EntityTypeBuilder<ProjectProperty> builder)
        {
            builder.ToTable("ProjectProperties");

            builder.Property(p => p.Key).HasMaxLength(100);
            builder.Property(p => p.Value).HasMaxLength(100);

            builder.HasKey(p => new { p.Key, p.Value, p.Project });
        }
    }
}
