using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.AggregatesModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.EntityConfiguration
{
    class ProjectVisibleRuleEntityConfiguration : IEntityTypeConfiguration<ProjectVisibleRule>
    {
        public void Configure(EntityTypeBuilder<ProjectVisibleRule> builder)
        {
            builder.ToTable("ProjectVisibleRules")
                .HasKey(p => p.Id);
        }
    }
}
