﻿using Microsoft.EntityFrameworkCore;
using ProxyService.Core.Models;

namespace ProxyService.Database;

public class ProxiesDbContext(DbContextOptions<ProxiesDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Proxy>(entity =>
        {
            entity.ToTable("proxies");

            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
            entity.Property(p => p.Created).ValueGeneratedOnAdd();
            entity.HasIndex(p => new { p.Ip, p.Port }).IsUnique();
            entity.Ignore(p => p.IpPort);
        });

        modelBuilder.Entity<GettingMethod>(entity =>
        {
            entity.ToTable("getting_methods");

            entity.HasKey(gm => gm.Id);
            entity.Property(gm => gm.Id).ValueGeneratedOnAdd();
            entity.HasIndex(gm => gm.Name).IsUnique();

            entity.HasData(
                new GettingMethod() { Id = 1, Name = "TextSpysOne", Description = "Downloads ~400 proxies from txt file", IsDisabled = false },
                new GettingMethod() { Id = 2, Name = "HttpsSpysOne", Description = "Downloads 500 ssl proxies from html page", IsDisabled = false },
                new GettingMethod() { Id = 3, Name = "ProxyOrg", Description = "Downloads ~700 proxies from 4 html sub-pages", IsDisabled = false }
            );
        });

        modelBuilder.Entity<CheckingMethod>(entity =>
        {
            entity.ToTable("checking_methods");

            entity.HasKey(cm => cm.Id);
            entity.Property(cm => cm.Id).ValueGeneratedOnAdd();

            entity.HasData(
                new CheckingMethod() { Id = 1, Name = "Ping", Description = "Tcp", IsDisabled = false },
                new CheckingMethod() { Id = 2, Name = "Site", Description = "Https", TestTarget = "https://wtfismyip.com/text", IsDisabled = false },
                new CheckingMethod() { Id = 3, Name = "Site", Description = "Http", TestTarget = "http://ifconfig.io/ip", IsDisabled = false }
            );
        });

        modelBuilder.Entity<CheckingRun>(entity =>
        {
            entity.ToTable("checking_runs");

            entity.HasKey(cr => cr.Id);
            entity.Property(cr => cr.Id).ValueGeneratedOnAdd();
            entity.Property(cr => cr.Created).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<CheckingSession>(entity =>
        {
            entity.ToTable("checking_sessions");

            entity.HasKey(cms => cms.Id);
            entity.Property(cms => cms.Id).ValueGeneratedOnAdd();
            entity.Property(cms => cms.Created).ValueGeneratedOnAdd();

            entity.HasOne(cms => cms.CheckingMethod)
                .WithMany(cm => cm.CheckingSessions)
                .HasForeignKey(cms => cms.CheckingMethodId);

            entity.HasOne(cms => cms.CheckingRun)
                .WithMany(cr => cr.CheckingSessions)
                .HasForeignKey(cms => cms.CheckingRunId);
        });

        modelBuilder.Entity<CheckingResult>(entity =>
        {
            entity.ToTable("checking_results");

            entity.HasKey(cr => cr.Id);
            entity.Property(cr => cr.Id).ValueGeneratedOnAdd();
            entity.Property(cr => cr.Created).ValueGeneratedOnAdd();

            entity.HasOne(cr => cr.Proxy)
                .WithMany(p => p.CheckingResults)
                .HasForeignKey(cr => cr.ProxyId)
                .IsRequired(false);

            entity.HasOne(cr => cr.CheckingSession)
                .WithMany(cms => cms.CheckingResults)
                .HasForeignKey(cr => cr.CheckingSessionId);
        });
    }

    public DbSet<Proxy> Proxies { get; set; }
    public DbSet<GettingMethod> GettingMethods { get; set; }
    public DbSet<CheckingMethod> CheckingMethods { get; set; }
    public DbSet<CheckingResult> CheckingResults { get; set; }
    public DbSet<CheckingSession> CheckingSessions { get; set; }
    public DbSet<CheckingRun> CheckingRuns { get; set; }
}
