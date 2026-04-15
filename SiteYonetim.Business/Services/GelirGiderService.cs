using Microsoft.EntityFrameworkCore;
using SiteYonetim.Core.DTOs;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;

namespace SiteYonetim.Business.Services;

public class GelirGiderService
{
    public void GelirKaydet(GelirKalemi gelir)
    {
        using var db = new AppDbContext();
        db.GelirKalemleri.Add(gelir);
        db.SaveChanges();
    }

    public void GelirGuncelle(GelirKalemi gelir)
    {
        using var db = new AppDbContext();
        db.GelirKalemleri.Update(gelir);
        db.SaveChanges();
    }

    public void GelirSil(int id)
    {
        using var db = new AppDbContext();
        var gelir = db.GelirKalemleri.Find(id);
        if (gelir != null)
        {
            db.GelirKalemleri.Remove(gelir);
            db.SaveChanges();
        }
    }

    public List<GelirKalemi> GelirListele(DateTime? bas = null, DateTime? bit = null, string? kategori = null)
    {
        using var db = new AppDbContext();
        var query = db.GelirKalemleri.AsQueryable();
        if (bas.HasValue) query = query.Where(g => g.Tarih >= bas.Value);
        if (bit.HasValue) query = query.Where(g => g.Tarih <= bit.Value);
        if (!string.IsNullOrEmpty(kategori) && kategori != "Tümü")
            query = query.Where(g => g.Kategori == kategori);
        return query.OrderByDescending(g => g.Tarih).ToList();
    }

    public void GiderKaydet(GiderKalemi gider)
    {
        using var db = new AppDbContext();
        db.GiderKalemleri.Add(gider);
        db.SaveChanges();
    }

    public void GiderGuncelle(GiderKalemi gider)
    {
        using var db = new AppDbContext();
        db.GiderKalemleri.Update(gider);
        db.SaveChanges();
    }

    public void GiderSil(int id)
    {
        using var db = new AppDbContext();
        var gider = db.GiderKalemleri.Find(id);
        if (gider != null)
        {
            db.GiderKalemleri.Remove(gider);
            db.SaveChanges();
        }
    }

    public List<GiderKalemi> GiderListele(DateTime? bas = null, DateTime? bit = null, string? kategori = null)
    {
        using var db = new AppDbContext();
        var query = db.GiderKalemleri.AsQueryable();
        if (bas.HasValue) query = query.Where(g => g.Tarih >= bas.Value);
        if (bit.HasValue) query = query.Where(g => g.Tarih <= bit.Value);
        if (!string.IsNullOrEmpty(kategori) && kategori != "Tümü")
            query = query.Where(g => g.Kategori == kategori);
        return query.OrderByDescending(g => g.Tarih).ToList();
    }

    public (decimal toplamGelir, decimal toplamGider, decimal net) KasaDurumu(DateTime bas, DateTime bit)
    {
        using var db = new AppDbContext();
        var gelir = db.GelirKalemleri.Where(g => g.Tarih >= bas && g.Tarih <= bit).Select(g => g.Tutar).ToList().Sum();
        var gider = db.GiderKalemleri.Where(g => g.Tarih >= bas && g.Tarih <= bit).Select(g => g.Tutar).ToList().Sum();
        return (gelir, gider, gelir - gider);
    }

    public OzetDTO AylikOzet(int yil, int ay)
    {
        using var db = new AppDbContext();
        var ayBas = new DateTime(yil, ay, 1);
        var ayBit = ayBas.AddMonths(1).AddDays(-1);

        var gelir = db.GelirKalemleri.Where(g => g.Tarih >= ayBas && g.Tarih <= ayBit).Select(g => g.Tutar).ToList().Sum();
        var gider = db.GiderKalemleri.Where(g => g.Tarih >= ayBas && g.Tarih <= ayBit).Select(g => g.Tutar).ToList().Sum();

        var tahakkuklar = db.AidatTahakkuklar.Where(t => t.Yil == yil && t.Ay == ay).ToList();
        var toplamTahakkuk = tahakkuklar.Sum(t => t.Tutar);
        var toplamTahsilat = tahakkuklar.Sum(t => t.OdenenTutar);
        var borcluSayisi = tahakkuklar.Count(t => t.Durum != Core.Enums.TahakkukDurumu.Odenmis);

        return new OzetDTO
        {
            ToplamGelir = gelir,
            ToplamGider = gider,
            AidatTahsilati = toplamTahsilat,
            ToplamTahakkuk = toplamTahakkuk,
            Net = gelir - gider,
            BorcluDaireSayisi = borcluSayisi
        };
    }
}
