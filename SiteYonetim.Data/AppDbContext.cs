using Microsoft.EntityFrameworkCore;
using SiteYonetim.Data.Entities;

namespace SiteYonetim.Data;

public class AppDbContext : DbContext
{
    public DbSet<Blok> Bloklar { get; set; } = null!;
    public DbSet<Daire> Daireler { get; set; } = null!;
    public DbSet<Sakin> Sakinler { get; set; } = null!;
    public DbSet<AidatTipi> AidatTipleri { get; set; } = null!;
    public DbSet<AidatTahakkuk> AidatTahakkuklar { get; set; } = null!;
    public DbSet<AidatTahsilat> AidatTahsilatlar { get; set; } = null!;
    public DbSet<GelirKalemi> GelirKalemleri { get; set; } = null!;
    public DbSet<GiderKalemi> GiderKalemleri { get; set; } = null!;
    public DbSet<SiteAyar> SiteAyarlar { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder opt)
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SiteYonetim");
        Directory.CreateDirectory(folder);
        opt.UseSqlite($"Data Source={Path.Combine(folder, "siteYonetim.db")}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blok>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Ad).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Daire>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DaireNo).IsRequired().HasMaxLength(20);
            e.HasOne(x => x.Blok).WithMany(b => b.Daireler).HasForeignKey(x => x.BlokId);
        });

        modelBuilder.Entity<Sakin>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Ad).IsRequired().HasMaxLength(100);
            e.Property(x => x.Soyad).IsRequired().HasMaxLength(100);
            e.HasOne(x => x.Daire).WithMany(d => d.Sakinler).HasForeignKey(x => x.DaireId);
        });

        modelBuilder.Entity<AidatTipi>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Ad).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<AidatTahakkuk>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Tutar).HasColumnType("decimal(18,2)");
            e.Property(x => x.OdenenTutar).HasColumnType("decimal(18,2)");
            e.Property(x => x.GecikmeTazminati).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Daire).WithMany(d => d.Tahakkuklar).HasForeignKey(x => x.DaireId);
            e.HasOne(x => x.AidatTipi).WithMany(a => a.Tahakkuklar).HasForeignKey(x => x.AidatTipiId);
        });

        modelBuilder.Entity<AidatTahsilat>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Tutar).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Tahakkuk).WithMany(t => t.Tahsilatlar).HasForeignKey(x => x.TahakkukId);
        });

        modelBuilder.Entity<GelirKalemi>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Tutar).HasColumnType("decimal(18,2)");
            e.Property(x => x.Aciklama).IsRequired().HasMaxLength(500);
        });

        modelBuilder.Entity<GiderKalemi>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Tutar).HasColumnType("decimal(18,2)");
            e.Property(x => x.Aciklama).IsRequired().HasMaxLength(500);
        });

        modelBuilder.Entity<SiteAyar>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Anahtar).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Anahtar).IsUnique();
        });
    }
}
