using Microsoft.EntityFrameworkCore;
using SiteYonetim.Core.DTOs;
using SiteYonetim.Core.Enums;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;

namespace SiteYonetim.Business.Services;

public class AidatService
{
    public (int olusturulan, string mesaj) TopluTahakkukOlustur(int yil, int ay, int aidatTipiId, decimal birimTutar)
    {
        using var db = new AppDbContext();

        var mevcutDaireler = db.AidatTahakkuklar
            .Where(t => t.Yil == yil && t.Ay == ay && t.AidatTipiId == aidatTipiId)
            .Select(t => t.DaireId)
            .ToHashSet();

        var aktifDaireler = db.Daireler.ToList();
        int sayac = 0;

        foreach (var daire in aktifDaireler)
        {
            if (mevcutDaireler.Contains(daire.Id))
                continue;

            var sonGun = new DateTime(yil, ay, DateTime.DaysInMonth(yil, ay));

            db.AidatTahakkuklar.Add(new AidatTahakkuk
            {
                DaireId = daire.Id,
                AidatTipiId = aidatTipiId,
                Yil = yil,
                Ay = ay,
                Tutar = birimTutar,
                OdenenTutar = 0,
                GecikmeTazminati = 0,
                Durum = TahakkukDurumu.Odenmemis,
                OlusturmaTarihi = DateTime.Now,
                SonOdemeTarihi = sonGun
            });
            sayac++;
        }

        db.SaveChanges();

        if (sayac == 0)
            return (0, $"{yil}/{ay} dönemi için bu aidat tipinde tahakkuk zaten mevcut.");

        return (sayac, $"{sayac} daire için tahakkuk oluşturuldu.");
    }

    public string TahsilatKaydet(int tahakkukId, decimal tutar, OdemeTipi tip, string? aciklama)
    {
        using var db = new AppDbContext();
        var tahakkuk = db.AidatTahakkuklar.Find(tahakkukId);
        if (tahakkuk == null) return "Tahakkuk bulunamadı!";

        db.AidatTahsilatlar.Add(new AidatTahsilat
        {
            TahakkukId = tahakkukId,
            Tutar = tutar,
            OdemeTarihi = DateTime.Now,
            OdemeTipi = tip,
            Aciklama = aciklama
        });

        tahakkuk.OdenenTutar += tutar;

        if (tahakkuk.OdenenTutar >= tahakkuk.Tutar + tahakkuk.GecikmeTazminati)
            tahakkuk.Durum = TahakkukDurumu.Odenmis;
        else if (tahakkuk.OdenenTutar > 0)
            tahakkuk.Durum = TahakkukDurumu.KismiOdeme;

        db.SaveChanges();
        return "Tahsilat kaydedildi.";
    }

    public string GecikmeTazminatiHesapla(int tahakkukId, decimal aylikFaizOrani)
    {
        using var db = new AppDbContext();
        var tahakkuk = db.AidatTahakkuklar.Find(tahakkukId);
        if (tahakkuk == null) return "Tahakkuk bulunamadı!";
        if (tahakkuk.Durum == TahakkukDurumu.Odenmis) return "Bu tahakkuk zaten ödenmiş.";
        if (tahakkuk.SonOdemeTarihi == null) return "Son ödeme tarihi belirlenmemiş.";

        if (DateTime.Now <= tahakkuk.SonOdemeTarihi.Value)
            return "Henüz vadesi geçmemiş.";

        int gecenAy = ((DateTime.Now.Year - tahakkuk.SonOdemeTarihi.Value.Year) * 12)
                      + DateTime.Now.Month - tahakkuk.SonOdemeTarihi.Value.Month;

        if (gecenAy <= 0) return "Henüz vadesi geçmemiş.";

        decimal kalanBorc = tahakkuk.Tutar - tahakkuk.OdenenTutar;
        decimal tazminat = kalanBorc * (aylikFaizOrani / 100m) * gecenAy;

        tahakkuk.GecikmeTazminati = Math.Round(tazminat, 2);
        db.SaveChanges();

        return $"Gecikme tazminatı: {tazminat:₺#,##0.00} ({gecenAy} ay)";
    }

    public List<BorcluDTO> AylikBorcluListesi(int yil, int ay)
    {
        using var db = new AppDbContext();
        return db.AidatTahakkuklar
            .Include(t => t.Daire).ThenInclude(d => d.Blok)
            .Include(t => t.Daire).ThenInclude(d => d.Sakinler)
            .Where(t => t.Yil == yil && t.Ay == ay && t.Durum != TahakkukDurumu.Odenmis)
            .AsEnumerable()
            .Select(t =>
            {
                var sakin = t.Daire.Sakinler.FirstOrDefault(s => s.Aktif);
                return new BorcluDTO
                {
                    BlokAd = t.Daire.Blok.Ad,
                    DaireNo = t.Daire.DaireNo,
                    SakinAd = sakin != null ? $"{sakin.Ad} {sakin.Soyad}" : "-",
                    BorcTutari = t.Tutar - t.OdenenTutar + t.GecikmeTazminati,
                    GecikmeTazminati = t.GecikmeTazminati,
                    Durum = t.Durum == TahakkukDurumu.KismiOdeme ? "Kısmi Ödeme" : "Ödenmemiş"
                };
            })
            .ToList();
    }

    public List<EkstreDTO> DaireEkstresi(int daireId, DateTime baslangic, DateTime bitis)
    {
        using var db = new AppDbContext();
        var basYil = baslangic.Year;
        var basAy = baslangic.Month;
        var bitYil = bitis.Year;
        var bitAy = bitis.Month;

        return db.AidatTahakkuklar
            .Include(t => t.AidatTipi)
            .Include(t => t.Tahsilatlar)
            .Where(t => t.DaireId == daireId)
            .Where(t => (t.Yil > basYil || (t.Yil == basYil && t.Ay >= basAy)) &&
                        (t.Yil < bitYil || (t.Yil == bitYil && t.Ay <= bitAy)))
            .OrderBy(t => t.Yil).ThenBy(t => t.Ay)
            .AsEnumerable()
            .Select(t => new EkstreDTO
            {
                Donem = $"{t.Yil}/{t.Ay:D2}",
                AidatTipi = t.AidatTipi.Ad,
                TahakkukTutar = t.Tutar,
                OdenenTutar = t.OdenenTutar,
                KalanTutar = t.Tutar - t.OdenenTutar + t.GecikmeTazminati,
                GecikmeTazminati = t.GecikmeTazminati,
                OdemeTarihi = t.Tahsilatlar.OrderByDescending(s => s.OdemeTarihi)
                    .FirstOrDefault()?.OdemeTarihi.ToString("dd.MM.yyyy")
            })
            .ToList();
    }
}
