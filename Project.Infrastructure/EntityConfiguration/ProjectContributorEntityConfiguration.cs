using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.AggregatesModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.EntityConfiguration
{
    class ProjectContributorEntityConfiguration : IEntityTypeConfiguration<ProjectContributor>
    {
        public void Configure(EntityTypeBuilder<ProjectContributor> builder)
        {
            builder.ToTable("ProjectContributors")
                .HasKey(p => p.Id);
        }
    }
}
