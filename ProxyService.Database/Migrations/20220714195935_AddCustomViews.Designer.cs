﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProxyService.Database;

#nullable disable

namespace ProxyService.Database.Migrations
{
    [DbContext(typeof(ProxiesDbContext))]
    [Migration("20220714195935_AddCustomViews")]
    partial class AddCustomViews
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("ProxyService.Core.Models.CheckingMethod", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("IsDisabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("TestTarget")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("checking_methods", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Tcp",
                            IsDisabled = false,
                            Name = "Ping"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Https",
                            IsDisabled = false,
                            Name = "Site",
                            TestTarget = "https://www.proxy-listen.de/azenv.php"
                        },
                        new
                        {
                            Id = 3,
                            Description = "Http",
                            IsDisabled = false,
                            Name = "Site",
                            TestTarget = "http://azenv.net/"
                        },
                        new
                        {
                            Id = 4,
                            Description = "Instagram",
                            IsDisabled = false,
                            Name = "Site",
                            TestTarget = "https://www.instagram.com/"
                        });
                });

            modelBuilder.Entity("ProxyService.Core.Models.CheckingMethodSession", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("CheckingMethodId")
                        .HasColumnType("int");

                    b.Property<int>("CheckingRunId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<int>("Elapsed")
                        .HasColumnType("int");

                    b.Property<bool>("Ignore")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("LocalhostResponseTime")
                        .HasColumnType("int");

                    b.Property<bool>("LocalhostResult")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("CheckingMethodId");

                    b.HasIndex("CheckingRunId");

                    b.ToTable("checking_method_sessions", (string)null);
                });

            modelBuilder.Entity("ProxyService.Core.Models.CheckingResult", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("CheckingMethodSessionId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("Ignore")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("ProxyId")
                        .HasColumnType("int");

                    b.Property<int>("ResponseTime")
                        .HasColumnType("int");

                    b.Property<bool>("Result")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("CheckingMethodSessionId");

                    b.HasIndex("ProxyId");

                    b.ToTable("checking_results", (string)null);
                });

            modelBuilder.Entity("ProxyService.Core.Models.CheckingRun", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<bool>("Ignore")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.ToTable("checking_runs", (string)null);
                });

            modelBuilder.Entity("ProxyService.Core.Models.GettingMethod", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("IsDisabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("getting_methods", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Downloads ~400 proxies from txt file",
                            IsDisabled = false,
                            Name = "SpysOne"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Downloads ~700 proxies from 4 html sub-pages",
                            IsDisabled = false,
                            Name = "ProxyOrg"
                        });
                });

            modelBuilder.Entity("ProxyService.Core.Models.Proxy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("Anonymity")
                        .HasColumnType("int");

                    b.Property<string>("CountryCode")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Ip")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("Modified")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Port")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Ip", "Port")
                        .IsUnique();

                    b.ToTable("proxies", (string)null);
                });

            modelBuilder.Entity("ProxyService.Core.Models.CheckingMethodSession", b =>
                {
                    b.HasOne("ProxyService.Core.Models.CheckingMethod", "CheckingMethod")
                        .WithMany("CheckingMethodSessions")
                        .HasForeignKey("CheckingMethodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProxyService.Core.Models.CheckingRun", "CheckingRun")
                        .WithMany("CheckingMethodSessions")
                        .HasForeignKey("CheckingRunId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CheckingMethod");

                    b.Navigation("CheckingRun");
                });

            modelBuilder.Entity("ProxyService.Core.Models.CheckingResult", b =>
                {
                    b.HasOne("ProxyService.Core.Models.CheckingMethodSession", "CheckingMethodSession")
                        .WithMany("CheckingResults")
                        .HasForeignKey("CheckingMethodSessionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProxyService.Core.Models.Proxy", "Proxy")
                        .WithMany("CheckingResults")
                        .HasForeignKey("ProxyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CheckingMethodSession");

                    b.Navigation("Proxy");
                });

            modelBuilder.Entity("ProxyService.Core.Models.CheckingMethod", b =>
                {
                    b.Navigation("CheckingMethodSessions");
                });

            modelBuilder.Entity("ProxyService.Core.Models.CheckingMethodSession", b =>
                {
                    b.Navigation("CheckingResults");
                });

            modelBuilder.Entity("ProxyService.Core.Models.CheckingRun", b =>
                {
                    b.Navigation("CheckingMethodSessions");
                });

            modelBuilder.Entity("ProxyService.Core.Models.Proxy", b =>
                {
                    b.Navigation("CheckingResults");
                });
#pragma warning restore 612, 618
        }
    }
}
