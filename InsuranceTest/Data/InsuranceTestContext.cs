using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InsuranceApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace InsuranceTest.Data
{
    public class InsuranceTestContext : IdentityDbContext
    {
        public InsuranceTestContext (DbContextOptions<InsuranceTestContext> options)
            : base(options)
        {
        }

        public DbSet<InsuranceApp.Models.Insurance> Insurance { get; set; } = default!;
        public DbSet<InsuranceApp.Models.InsuredEvent> InsuredEvent { get; set; }
        public DbSet<InsuranceApp.Models.Insured> Insured { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Decimal data types precision
            builder.Entity<Insurance>().Property(x => x.Amount).HasColumnType("decimal(10,2)");
            builder.Entity<InsuredEvent>().Property(x => x.Amount).HasColumnType("decimal(10,2)");
        }
    }
}
