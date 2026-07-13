using Microsoft.EntityFrameworkCore;
using QalatAldhaman.Store.Api.Entities;
using QalatAldhaman.Store.Api.Entities.Enums;

namespace QalatAldhaman.Store.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    /// <summary>DB sequence backing OrderNumber generation (see Services/OrderNumberGenerator.cs).</summary>
    public const string OrderNumberSequenceName = "order_number_seq";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasSequence<long>(OrderNumberSequenceName).StartsAt(1).IncrementsBy(1);

        // Money-like columns: numeric(12,2). GPS coordinates: numeric(9,6).
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.Property(c => c.MinInvoiceCash).HasColumnType("numeric(12,2)");
            entity.Property(c => c.MinInvoiceInstallment).HasColumnType("numeric(12,2)");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.CashPrice).HasColumnType("numeric(12,2)");
            entity.Property(p => p.MonthlyInstallmentPrice).HasColumnType("numeric(12,2)");
            entity.Property(p => p.DailyInstallmentPrice).HasColumnType("numeric(12,2)");

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasIndex(a => a.Username).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(o => o.OrderNumber).IsUnique();

            entity.Property(o => o.PriceSnapshot).HasColumnType("numeric(12,2)");
            entity.Property(o => o.GpsLat).HasColumnType("numeric(9,6)");
            entity.Property(o => o.GpsLng).HasColumnType("numeric(9,6)");

            entity.Property(o => o.PurchaseMethod).HasConversion<string>().HasMaxLength(30);
            entity.Property(o => o.MediaType).HasConversion<string>().HasMaxLength(20);
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(o => o.Product)
                .WithMany()
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Category)
                .WithMany()
                .HasForeignKey(o => o.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Governorate)
                .WithMany()
                .HasForeignKey(o => o.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed: the 12 governorates, in the exact required display order (Id doubles as DisplayOrder).
        modelBuilder.Entity<Governorate>().HasData(
            new Governorate { Id = 1, Name = "بغداد الكرخ" },
            new Governorate { Id = 2, Name = "بغداد الرصافة" },
            new Governorate { Id = 3, Name = "النجف الأشرف" },
            new Governorate { Id = 4, Name = "البصرة" },
            new Governorate { Id = 5, Name = "كربلاء" },
            new Governorate { Id = 6, Name = "بابل" },
            new Governorate { Id = 7, Name = "سماوة (المثنى)" },
            new Governorate { Id = 8, Name = "الناصرية" },
            new Governorate { Id = 9, Name = "الديوانية" },
            new Governorate { Id = 10, Name = "ديالى" },
            new Governorate { Id = 11, Name = "الموصل" },
            new Governorate { Id = 12, Name = "كركوك" }
        );
    }
}
