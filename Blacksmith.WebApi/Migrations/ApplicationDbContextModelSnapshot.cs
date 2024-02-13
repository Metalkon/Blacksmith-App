﻿// <auto-generated />
using System;
using Blacksmith.WebApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Blacksmith.WebApi.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Blacksmith.WebApi.Models.RefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("TokenExp")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("Blacksmith.WebApi.Models.UserModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LoginCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LoginCodeExp")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LoginHistory")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Shared_Classes.Models.TestPotato", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("TestPotatoes");
                });

            modelBuilder.Entity("Blacksmith.WebApi.Models.RefreshToken", b =>
                {
                    b.HasOne("Blacksmith.WebApi.Models.UserModel", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Blacksmith.WebApi.Models.UserModel", b =>
                {
                    b.OwnsOne("Blacksmith.WebApi.Models.AccountStatus", "AccountStatus", b1 =>
                        {
                            b1.Property<int>("UserModelId")
                                .HasColumnType("int");

                            b1.Property<string>("Status")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<DateTime?>("Time")
                                .HasColumnType("datetime2");

                            b1.Property<bool>("Validated")
                                .HasColumnType("bit");

                            b1.Property<int?>("Value")
                                .HasColumnType("int");

                            b1.HasKey("UserModelId");

                            b1.ToTable("Users");

                            b1.ToJson("AccountStatus");

                            b1.WithOwner()
                                .HasForeignKey("UserModelId");
                        });

                    b.OwnsOne("Blacksmith.WebApi.Models.LoginStatus", "LoginStatus", b1 =>
                        {
                            b1.Property<int>("UserModelId")
                                .HasColumnType("int");

                            b1.Property<string>("History")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Status")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<DateTime?>("Time")
                                .HasColumnType("datetime2");

                            b1.Property<int?>("Value")
                                .HasColumnType("int");

                            b1.HasKey("UserModelId");

                            b1.ToTable("Users");

                            b1.ToJson("LoginStatus");

                            b1.WithOwner()
                                .HasForeignKey("UserModelId");
                        });

                    b.Navigation("AccountStatus")
                        .IsRequired();

                    b.Navigation("LoginStatus")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
