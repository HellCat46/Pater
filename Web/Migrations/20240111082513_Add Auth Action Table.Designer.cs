﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Web.ApplicationDbContext;

#nullable disable

namespace Web.Migrations
{
    [DbContext(typeof(UserDbContext))]
    [Migration("20240111082513_Add Auth Action Table")]
    partial class AddAuthActionTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Web.Models.Account.AccountModel", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnOrder(0);

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<string>("PicPath")
                        .HasMaxLength(41)
                        .HasColumnType("nvarchar(41)")
                        .HasColumnOrder(4);

                    b.Property<int>("Plan")
                        .HasColumnType("int");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("VARCHAR")
                        .HasColumnOrder(1);

                    b.Property<bool>("isAdmin")
                        .HasColumnType("bit")
                        .HasColumnOrder(5);

                    b.Property<bool>("isVerified")
                        .HasColumnType("bit")
                        .HasColumnOrder(6);

                    b.Property<string>("name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("VARCHAR")
                        .HasColumnOrder(3);

                    b.Property<string>("password")
                        .HasMaxLength(50)
                        .HasColumnType("VARCHAR")
                        .HasColumnOrder(2);

                    b.HasKey("id");

                    b.ToTable("Account");
                });

            modelBuilder.Entity("Web.Models.Account.ActivityLogModel", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IPAddr")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Userid")
                        .HasColumnType("int");

                    b.Property<DateTime>("date")
                        .HasColumnType("datetime2");

                    b.HasKey("id");

                    b.HasIndex("Userid");

                    b.ToTable("ActivityLogs");
                });

            modelBuilder.Entity("Web.Models.Account.AuthActionModel", b =>
                {
                    b.Property<string>("code")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("Userid")
                        .HasColumnType("int");

                    b.Property<int>("action")
                        .HasColumnType("int");

                    b.Property<DateTime>("createAt")
                        .HasColumnType("datetime2");

                    b.HasKey("code");

                    b.HasIndex("Userid");

                    b.ToTable("AuthAction");
                });

            modelBuilder.Entity("Web.Models.Account.ExternalAuthModel", b =>
                {
                    b.Property<string>("UserID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<int>("Provider")
                        .HasColumnType("int");

                    b.HasKey("UserID");

                    b.HasIndex("AccountId")
                        .IsUnique();

                    b.ToTable("ExternalAuth");
                });

            modelBuilder.Entity("Web.Models.Link.AnalyticsModel", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<string>("LinkModelCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("browser")
                        .HasColumnType("int");

                    b.Property<string>("city")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("country")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("device")
                        .HasColumnType("int");

                    b.Property<int>("os")
                        .HasColumnType("int");

                    b.Property<DateTime>("visitedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("id");

                    b.HasIndex("LinkModelCode");

                    b.ToTable("Analytics");
                });

            modelBuilder.Entity("Web.Models.Link.LinkModel", b =>
                {
                    b.Property<string>("code")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("code");

                    b.HasIndex("AccountId");

                    b.ToTable("Link");
                });

            modelBuilder.Entity("Web.Models.Account.ActivityLogModel", b =>
                {
                    b.HasOne("Web.Models.Account.AccountModel", "User")
                        .WithMany("Logs")
                        .HasForeignKey("Userid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Web.Models.Account.AuthActionModel", b =>
                {
                    b.HasOne("Web.Models.Account.AccountModel", "User")
                        .WithMany()
                        .HasForeignKey("Userid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Web.Models.Account.ExternalAuthModel", b =>
                {
                    b.HasOne("Web.Models.Account.AccountModel", "Account")
                        .WithOne("ExternalAuth")
                        .HasForeignKey("Web.Models.Account.ExternalAuthModel", "AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Web.Models.Link.AnalyticsModel", b =>
                {
                    b.HasOne("Web.Models.Link.LinkModel", "LinkModel")
                        .WithMany()
                        .HasForeignKey("LinkModelCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LinkModel");
                });

            modelBuilder.Entity("Web.Models.Link.LinkModel", b =>
                {
                    b.HasOne("Web.Models.Account.AccountModel", "Account")
                        .WithMany("Links")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Web.Models.Account.AccountModel", b =>
                {
                    b.Navigation("ExternalAuth")
                        .IsRequired();

                    b.Navigation("Links");

                    b.Navigation("Logs");
                });
#pragma warning restore 612, 618
        }
    }
}
