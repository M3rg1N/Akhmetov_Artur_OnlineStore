using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OnlineStore.Models;

public partial class OnlineStoreContext : DbContext
{
    public OnlineStoreContext()
    {
    }

    public OnlineStoreContext(DbContextOptions<OnlineStoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-5IPG7EG;Database=OnlineStore;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.IdCategory).HasName("PK__Categori__6DB3A68A8F10E20B");

            entity.Property(e => e.IdCategory).HasColumnName("ID_Category");
            entity.Property(e => e.NameCategory)
                .HasMaxLength(50)
                .HasColumnName("Name_Category");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.IdOrder).HasName("PK__Orders__EC9FA9557AB2E590");

            entity.Property(e => e.IdOrder).HasColumnName("ID_Order");
            entity.Property(e => e.DateOrder)
                .HasColumnType("datetime")
                .HasColumnName("Date_Order");
            entity.Property(e => e.DeliveryAddress)
                .HasMaxLength(255)
                .HasColumnName("Delivery_Address");
            entity.Property(e => e.IdOrderStatus).HasColumnName("ID_Order_Status");
            entity.Property(e => e.IdUser).HasColumnName("ID_User");

            entity.HasOne(d => d.IdOrderStatusNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdOrderStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Order_Statuses");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Users");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.IdOrderItem).HasName("PK__Order_It__BA5176FEE162BE53");

            entity.ToTable("Order_Items");

            entity.Property(e => e.IdOrderItem).HasColumnName("ID_Order_Item");
            entity.Property(e => e.IdOrder).HasColumnName("ID_Order");
            entity.Property(e => e.IdProduct).HasColumnName("ID_Product");
            entity.Property(e => e.PriceOrderItem)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Price_Order_Item");
            entity.Property(e => e.QuantityOrderItem).HasColumnName("Quantity_Order_Item");

            entity.HasOne(d => d.IdOrderNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.IdOrder)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order_Ite__ID_Or__4316F928");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.IdProduct)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order_Ite__ID_Pr__440B1D61");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.IdOrderStatus);

            entity.ToTable("Order_Statuses");

            entity.Property(e => e.IdOrderStatus).HasColumnName("ID_Order_Status");
            entity.Property(e => e.NameOrderStatus)
                .HasMaxLength(50)
                .HasColumnName("Name_Order_Status");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.IdProduct).HasName("PK__Products__522DE4963C920B0D");

            entity.Property(e => e.IdProduct).HasColumnName("ID_Product");
            entity.Property(e => e.IdCategory).HasColumnName("ID_Category");
            entity.Property(e => e.NameProduct)
                .HasMaxLength(100)
                .HasColumnName("Name_Product");
            entity.Property(e => e.PriceProduct)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Price_Product");
            entity.Property(e => e.QuantityProduct).HasColumnName("Quantity_Product");

            entity.HasOne(d => d.IdCategoryNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.IdCategory)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Products__ID_Cat__3B75D760");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole);

            entity.Property(e => e.IdRole).HasColumnName("ID_Role");
            entity.Property(e => e.NameRole)
                .HasMaxLength(50)
                .HasColumnName("Name_Role");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser);

            entity.HasIndex(e => e.Login, "UQ_Users_Login").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("First_Name");
            entity.Property(e => e.HashPassword)
                .HasMaxLength(255)
                .HasColumnName("Hash_Password");
            entity.Property(e => e.IdRole).HasColumnName("ID_Role");
            entity.Property(e => e.IsAddressConfirmed)
                .HasDefaultValue(false, "DF_Users_AddressConfirmed")
                .HasColumnName("Is_Address_Confirmed");
            entity.Property(e => e.IsEmailVerified)
                .HasDefaultValue(false, "DF_Users_EmailVerified")
                .HasColumnName("Is_Email_Verified");
            entity.Property(e => e.IsPhoneVerified)
                .HasDefaultValue(false, "DF_Users_PhoneVerified")
                .HasColumnName("Is_Phone_Verified");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("Last_Name");
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .HasColumnName("Middle_Name");
            entity.Property(e => e.Phone).HasMaxLength(50);

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
