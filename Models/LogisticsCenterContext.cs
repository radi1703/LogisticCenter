using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LogisticsSystem.Models
{
    public partial class LogisticsCenterContext : DbContext
    {
        public LogisticsCenterContext()
        {
        }

        public LogisticsCenterContext(DbContextOptions<LogisticsCenterContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Courier> Couriers { get; set; } = null!;
        public virtual DbSet<Shipment> Shipments { get; set; } = null!;
        public DbSet<SystemUser> SystemUsers { get; set; }
        public virtual DbSet<Warehouse> Warehouses { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- КУРИЕРИ ---
            modelBuilder.Entity<Courier>(entity =>
            {
                entity.ToTable("Couriers");
                entity.HasKey(e => e.CourierId);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Username).HasMaxLength(50);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Zone).HasMaxLength(50);
            });

            // --- ПРАТКИ ---
            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.ToTable("Shipments");
                entity.HasKey(e => e.ShipmentId);

                entity.Property(e => e.TrackingNumber).HasMaxLength(50);
                entity.Property(e => e.Weight).HasColumnType("decimal(18, 2)");

                // връзката е чрез CurrentWarehouseId
                entity.HasOne(d => d.CurrentWarehouse)
                      .WithMany()
                      .HasForeignKey(d => d.CurrentWarehouseId) 
                      .HasConstraintName("FK_Shipments_Warehouses");

                entity.HasOne(d => d.AssignedCourier)
                      .WithMany()
                      .HasForeignKey(d => d.AssignedCourierId)
                      .HasConstraintName("FK_Shipments_Couriers");
            });

            // --- ДРУГИ ---
            modelBuilder.Entity<Warehouse>(entity => { entity.HasKey(e => e.WarehouseId); });
            modelBuilder.Entity<User>(entity => { entity.HasKey(e => e.UserId); });
            modelBuilder.Entity<Role>(entity => { entity.HasKey(e => e.RoleId); });

            modelBuilder.Entity<Shipment>().Ignore("WarehouseId");
            modelBuilder.Entity<Shipment>().Ignore("Warehouse");

            try { modelBuilder.Entity<Shipment>().Ignore("WarehouseId"); } catch {}
            try { modelBuilder.Entity<Shipment>().Ignore("Warehouse"); } catch {}

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}