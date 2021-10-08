using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace julieta.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        private IWebHostEnvironment _env;


        public DataContext(DbContextOptions<DataContext> options, IWebHostEnvironment env)
            : base(options)
        {
            _env = env;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
        }


    }


}