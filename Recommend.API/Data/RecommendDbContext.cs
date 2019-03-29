using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore;
using Recommend.API.Models;

namespace Recommend.API.Data
{
    public class RecommendDbContext : DbContext
    {
        public RecommendDbContext(DbContextOptions<RecommendDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectRecommend>().ToTable("ProjectRecommends")
                .HasKey(t => t.Id);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<ProjectRecommend> ProjectRecommends { get; set; }
    }
}
