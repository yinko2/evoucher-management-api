using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace eVoucherAPI.Models
{
    public partial class eVoucherContext : DbContext
    {
        private IConfiguration _configuration;
        private string _connectionString;
        public readonly IHttpContextAccessor _httpContextAccessor;

        public eVoucherContext(DbContextOptions<eVoucherContext> options, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
            : base(options)
        {
             _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _connectionString = _configuration["ConnectionStrings:DefaultConnection"];
        }

        public virtual DbSet<Buytype> BuyTypes { get; set; }
        public virtual DbSet<Evoucher> Evouchers { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Purchase> Purchases { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Eventlogs> Eventlogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(_connectionString, Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.11-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
