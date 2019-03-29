using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.AggregatesModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.EntityConfiguration
{
    class ProjectEntityConfiguration : IEntityTypeConfiguration<Domain.AggregatesModel.Project>
    {
        public void Configure(EntityTypeBuilder<Domain.AggregatesModel.Project> builder)
        {
            builder.ToTable("Projects")
                .HasKey(p => p.Id);
        }
    }
}
