using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class AppContext: DbContext
    {
        public DbSet<Price> Prices { get; set; }

        private readonly string connectionString;

        public AppContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(@"Server=localhost;Database=US;Trusted_Connection=True;Trust Server Certificate=True;User Id=admin;Password=admin;");
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Price>()
                .HasKey(p => new { p.Ticker, p.TimeFrame, p.Date });
        }
    }
}
