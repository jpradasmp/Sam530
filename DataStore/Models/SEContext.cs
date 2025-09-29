using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public partial class SEContext : DbContext
    {
        public SEContext()
        {
        }

        public SEContext(DbContextOptions<SEContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Setups> Setups { get; set; }
        public virtual DbSet<Radius> Radius { get; set; }
        public virtual DbSet<Syslog> Syslog { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(SqliteConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Setups>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });
            modelBuilder.Entity<Radius>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });
            modelBuilder.Entity<Syslog>(entity =>
            {
                entity.HasKey(e => new { e.Id });
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
