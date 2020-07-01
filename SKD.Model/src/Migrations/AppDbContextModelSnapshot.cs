﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SKD.Model;

namespace SKD.Model.src.Migrations
{
    [DbContext(typeof(SkdContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SKD.Model.Component", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasMaxLength(36);

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(10)")
                        .HasMaxLength(10);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("component");
                });

            modelBuilder.Entity("SKD.Model.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasMaxLength(36);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(320)")
                        .HasMaxLength(320);

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("user");
                });

            modelBuilder.Entity("SKD.Model.Vehicle", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasMaxLength(36);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("KitNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LotNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ModelId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("VIN")
                        .IsRequired()
                        .HasColumnType("nvarchar(17)")
                        .HasMaxLength(17);

                    b.HasKey("Id");

                    b.HasIndex("ModelId");

                    b.HasIndex("VIN")
                        .IsUnique();

                    b.ToTable("vehicle");
                });

            modelBuilder.Entity("SKD.Model.VehicleComponent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasMaxLength(36);

                    b.Property<Guid>("ComponentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Scan1")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Scan2")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("ScanAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Sequence")
                        .HasColumnType("int");

                    b.Property<Guid>("VehicleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ComponentId");

                    b.HasIndex("Scan1");

                    b.HasIndex("Scan2");

                    b.HasIndex("VehicleId", "ComponentId")
                        .IsUnique();

                    b.ToTable("vehicle_component");
                });

            modelBuilder.Entity("SKD.Model.VehicleModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasMaxLength(36);

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(11)")
                        .HasMaxLength(11);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("vehicle_model");
                });

            modelBuilder.Entity("SKD.Model.VehicleModelComponent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasMaxLength(36);

                    b.Property<Guid>("ComponentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("PrerequisiteSequences")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<DateTime?>("RemovedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Sequence")
                        .HasColumnType("int");

                    b.Property<Guid>("VehicleModelId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ComponentId");

                    b.HasIndex("VehicleModelId", "ComponentId")
                        .IsUnique();

                    b.ToTable("vehicle_model_component");
                });

            modelBuilder.Entity("SKD.Model.Vehicle", b =>
                {
                    b.HasOne("SKD.Model.VehicleModel", "Model")
                        .WithMany("Vehicles")
                        .HasForeignKey("ModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SKD.Model.VehicleComponent", b =>
                {
                    b.HasOne("SKD.Model.Component", "Component")
                        .WithMany("VehicleComponents")
                        .HasForeignKey("ComponentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SKD.Model.Vehicle", "Vehicle")
                        .WithMany("VehicleComponents")
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SKD.Model.VehicleModelComponent", b =>
                {
                    b.HasOne("SKD.Model.Component", "Component")
                        .WithMany("VehicleModelComponents")
                        .HasForeignKey("ComponentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SKD.Model.VehicleModel", "VehicleModel")
                        .WithMany("ComponentMappings")
                        .HasForeignKey("VehicleModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
