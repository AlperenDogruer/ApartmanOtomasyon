using System.Data;
using Microsoft.EntityFrameworkCore;
using SiteYonetim.Core.Enums;
using SiteYonetim.Data;

namespace SiteYonetim.Business.Services;

public class RaporService
{
    public DataTable BorcluListesiRaporu(int yil, int ay)
    {
        var dt = new DataTable();
        dt.Columns.Add("Blok", typeof(string));
        dt.Columns.Add("Daire No", typeof(string));
        dt.Columns.Add("Sakin", typeof(string));
        dt.Columns.Add("Borç Tutarı", typeof(decimal));
        dt.Columns.Add("Gecikme Tazminatı", typeof(decimal));
        dt.Columns.Add("Durum", typeof(string));

        using var db = new AppDbContext();
        var list = db.AidatTahakkuklar
            .Include(t => t.Daire).ThenInclude(d => d.Blok)
            .Include(t => t.Daire).ThenInclude(d => d.Sakinler)
            .Where(t => t.Yil == yil && t.Ay == ay && t.Durum != TahakkukDurumu.Odenmis)
            .ToList();

        foreach (var t in list)
        {
            var sakin = t.Daire.Sakinler.FirstOrDefault(s => s.Aktif);
            dt.Rows.Add(
                t.Daire.Blok.Ad,
                t.Daire.DaireNo,
                sakin != null ? $"{sakin.Ad} {sakin.Soyad}" : "-",
                t.Tutar - t.OdenenTutar,
                t.GecikmeTazminati,
                t.Durum == TahakkukDurumu.KismiOdeme ? "Kısmi Ödeme" : "Ödenmemiş"
            );
        }
        return dt;
    }

    public DataTable DaireEkstreRaporu(int daireId, DateTime bas, DateTime bit)
    {
        var dt = new DataTable();
        dt.Columns.Add("Dönem", typeof(string));
        dt.Columns.Add("Aidat Tipi", typeof(string));
        dt.Columns.Add("Tahakkuk", typeof(decimal));
        dt.Columns.Add("Ödenen", typeof(decimal));
        dt.Columns.Add("Kalan", typeof(decimal));
        dt.Columns.Add("Gecikme Taz.", typeof(decimal));

        using var db = new AppDbContext();
        var list = db.AidatTahakkuklar
            .Include(t => t.AidatTipi)
            .Where(t => t.DaireId == daireId)
            .Where(t => (t.Yil > bas.Year || (t.Yil == bas.Year && t.Ay >= bas.Month)) &&
                        (t.Yil < bit.Year || (t.Yil == bit.Year && t.Ay <= bit.Month)))
            .OrderBy(t => t.Yil).ThenBy(t => t.Ay)
            .ToList();

        foreach (var t in list)
        {
            dt.Rows.Add(
                $"{t.Yil}/{t.Ay:D2}",
                t.AidatTipi.Ad,
                t.Tutar,
                t.OdenenTutar,
                t.Tutar - t.OdenenTutar + t.GecikmeTazminati,
                t.GecikmeTazminati
            );
        }
        return dt;
    }

    public DataTable YillikMizanRaporu(int yil)
    {
        var dt = new DataTable();
        dt.Columns.Add("Ay", typeof(string));
        dt.Columns.Add("Gelir", typeof(decimal));
        dt.Columns.Add("Gider", typeof(decimal));
        dt.Columns.Add("Net", typeof(decimal));

        using var db = new AppDbContext();
        string[] aylar = { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
                           "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

        for (int ay = 1; ay <= 12; ay++)
        {
            var ayBas = new DateTime(yil, ay, 1);
            var ayBit = ayBas.AddMonths(1).AddDays(-1);

            var gelir = db.GelirKalemleri.Where(g => g.Tarih >= ayBas && g.Tarih <= ayBit).Select(g => g.Tutar).ToList().Sum();
            var gider = db.GiderKalemleri.Where(g => g.Tarih >= ayBas && g.Tarih <= ayBit).Select(g => g.Tutar).ToList().Sum();

            dt.Rows.Add(aylar[ay - 1], gelir, gider, gelir - gider);
        }
        return dt;
    }

    public DataTable GelirGiderRaporu(DateTime bas, DateTime bit)
    {
        var dt = new DataTable();
        dt.Columns.Add("Tür", typeof(string));
        dt.Columns.Add("Tarih", typeof(string));
        dt.Columns.Add("Açıklama", typeof(string));
        dt.Columns.Add("Kategori", typeof(string));
        dt.Columns.Add("Tutar", typeof(decimal));

        using var db = new AppDbContext();

        var gelirler = db.GelirKalemleri
            .Where(g => g.Tarih >= bas && g.Tarih <= bit)
            .OrderBy(g => g.Tarih)
            .ToList();

        foreach (var g in gelirler)
            dt.Rows.Add("Gelir", g.Tarih.ToString("dd.MM.yyyy"), g.Aciklama, g.Kategori, g.Tutar);

        var giderler = db.GiderKalemleri
            .Where(g => g.Tarih >= bas && g.Tarih <= bit)
            .OrderBy(g => g.Tarih)
            .ToList();

        foreach (var g in giderler)
            dt.Rows.Add("Gider", g.Tarih.ToString("dd.MM.yyyy"), g.Aciklama, g.Kategori, g.Tutar);

        return dt;
    }
}
