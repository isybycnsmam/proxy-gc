using Microsoft.EntityFrameworkCore;
using ProxyService.Core.Models;

namespace ProxyService.Database
{
    public class ProxiesDbContext : DbContext
    {
        public ProxiesDbContext(DbContextOptions<ProxiesDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Proxy>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.Created).ValueGeneratedOnAdd();
                entity.HasIndex(p => new { p.Ip, p.Port }).IsUnique();
                entity.Ignore(p => p.IpPort);
            });

            modelBuilder.Entity<GettingMethod>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.HasIndex(p => p.Name).IsUnique();

                entity.HasData(
                    new GettingMethod() { Id = 1, Name = "SpysOne", Description = "Downloads ~400 proxies from txt file", IsDisabled = false },
                    new GettingMethod() { Id = 2, Name = "ProxyOrg", Description = "Downloads ~700 proxies from 4 html sub-pages", IsDisabled = false }
                );
            });

            modelBuilder.Entity<CheckingMethod>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();

                entity.HasData(
                    new CheckingMethod() { Id = 1, Name = "Ping", Description = "Tcp", TestTarget = "", IsDisabled = false },
                    new CheckingMethod() { Id = 2, Name = "Site", Description = "Https", TestTarget = "https://www.proxy-listen.de/azenv.php", IsDisabled = false },
                    new CheckingMethod() { Id = 3, Name = "Site", Description = "Http", TestTarget = "http://azenv.net/", IsDisabled = false },
                    new CheckingMethod() { Id = 4, Name = "Site", Description = "Instagram", TestTarget = "https://www.instagram.com/", IsDisabled = false }
                );
            });

            modelBuilder.Entity<CheckingResult>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.Created).ValueGeneratedOnAdd();

                entity.HasOne(cr => cr.Proxy)
                    .WithMany(p => p.CheckingResults)
                    .HasForeignKey(cr => cr.ProxyId);

                entity.HasOne(cr => cr.CheckingMethod)
                    .WithMany(cm => cm.CheckingResults)
                    .HasForeignKey(cr => cr.CheckingMethodId);
            });
        }

        public DbSet<Proxy> Proxies { get; set; }
        public DbSet<GettingMethod> GettingMethods { get; set; }
        public DbSet<CheckingMethod> CheckingMethods { get; set; }
        public DbSet<CheckingResult> CheckingResults { get; set; }
    }
}
