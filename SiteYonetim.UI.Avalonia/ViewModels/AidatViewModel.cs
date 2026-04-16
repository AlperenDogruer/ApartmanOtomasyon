using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SiteYonetim.Business.Services;
using SiteYonetim.Core.Enums;
using SiteYonetim.Data;
using SiteYonetim.UI.Avalonia.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SiteYonetim.UI.Avalonia.ViewModels;

public partial class AidatViewModel : ViewModelBase
{
    public static readonly string[] Aylar = { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

    [ObservableProperty] private int _yil = DateTime.Now.Year;
    [ObservableProperty] private int _ayIndex = DateTime.Now.Month - 1;

    // Tab 2 - Ekstre
    [ObservableProperty] private DaireItem? _selectedDaire;
    [ObservableProperty] private DateTime _ekstreBas = new(DateTime.Now.Year, 1, 1);
    [ObservableProperty] private DateTime _ekstreBit = DateTime.Now;

    // Tab 3 - Gecikme
    [ObservableProperty] private int _yil3 = DateTime.Now.Year;
    [ObservableProperty] private int _ay3Index = DateTime.Now.Month - 1;

    public ObservableCollection<string> AylarList { get; } = new(Aylar);
    public ObservableCollection<TahakkukRow> Tahakkuklar { get; } = new();
    public ObservableCollection<EkstreRow> Ekstre { get; } = new();
    public ObservableCollection<GecikmeRow> GecikmeList { get; } = new();
    public ObservableCollection<DaireItem> Daireler { get; } = new();

    public event EventHandler? RequestTopluTahakkukDialog;
    public event EventHandler? RequestTahsilatGirDialog;
    public event EventHandler<string>? RequestSaveFile;

    public AidatViewModel()
    {
        LoadDaireler();
        LoadTahakkuklar();
    }

    private void LoadDaireler()
    {
        try
        {
            using var db = new AppDbContext();
            foreach (var d in db.Daireler.Include(x => x.Blok)
                         .OrderBy(x => x.Blok.Ad).ThenBy(x => x.DaireNo).ToList())
                Daireler.Add(new DaireItem { Id = d.Id, Display = $"{d.Blok.Ad} - Daire {d.DaireNo}" });
            if (Daireler.Count > 0) SelectedDaire = Daireler[0];
        }
        catch { }
    }

    [RelayCommand]
    public void LoadTahakkuklar()
    {
        Tahakkuklar.Clear();
        try
        {
            int ay = AyIndex + 1;
            using var db = new AppDbContext();
            var list = db.AidatTahakkuklar
                .Include(t => t.Daire).ThenInclude(d => d.Blok)
                .Include(t => t.Daire).ThenInclude(d => d.Sakinler)
                .Include(t => t.AidatTipi)
                .Where(t => t.Yil == Yil && t.Ay == ay)
                .OrderBy(t => t.Daire.Blok.Ad).ThenBy(t => t.Daire.DaireNo)
                .ToList();
            foreach (var t in list)
            {
                var sakin = t.Daire.Sakinler.FirstOrDefault(s => s.Aktif);
                Tahakkuklar.Add(new TahakkukRow
                {
                    Id = t.Id,
                    Blok = t.Daire.Blok.Ad,
                    DaireNo = t.Daire.DaireNo,
                    Sakin = sakin != null ? $"{sakin.Ad} {sakin.Soyad}" : "-",
                    AidatTipi = t.AidatTipi.Ad,
                    Tutar = t.Tutar,
                    Odenen = t.OdenenTutar,
                    GecikmeTaz = t.GecikmeTazminati,
                    Durum = t.Durum == TahakkukDurumu.Odenmis ? "Ödenmiş"
                          : t.Durum == TahakkukDurumu.KismiOdeme ? "Kısmi Ödeme" : "Ödenmemiş"
                });
            }
        }
        catch (Exception ex) { _ = DialogService.ShowErrorAsync($"Tahakkuk listesi yüklenirken hata: {ex.Message}"); }
    }

    [RelayCommand]
    public void LoadEkstre()
    {
        Ekstre.Clear();
        if (SelectedDaire == null) return;
        try
        {
            var svc = new AidatService();
            foreach (var e in svc.DaireEkstresi(SelectedDaire.Id, EkstreBas, EkstreBit))
            {
                Ekstre.Add(new EkstreRow
                {
                    Donem = e.Donem, AidatTipi = e.AidatTipi,
                    Tahakkuk = e.TahakkukTutar, Odenen = e.OdenenTutar, Kalan = e.KalanTutar,
                    GecikmeTaz = e.GecikmeTazminati, OdemeTarihi = e.OdemeTarihi ?? "-"
                });
            }
        }
        catch (Exception ex) { _ = DialogService.ShowErrorAsync($"Ekstre yüklenirken hata: {ex.Message}"); }
    }

    [RelayCommand]
    public void LoadGecikme()
    {
        GecikmeList.Clear();
        try
        {
            int ay = Ay3Index + 1;
            using var db = new AppDbContext();
            var list = db.AidatTahakkuklar
                .Include(t => t.Daire).ThenInclude(d => d.Blok)
                .Where(t => t.Yil == Yil3 && t.Ay == ay && t.Durum != TahakkukDurumu.Odenmis)
                .Where(t => t.SonOdemeTarihi != null && t.SonOdemeTarihi < DateTime.Now)
                .OrderBy(t => t.Daire.Blok.Ad).ThenBy(t => t.Daire.DaireNo)
                .ToList();
            foreach (var t in list)
            {
                GecikmeList.Add(new GecikmeRow
                {
                    Id = t.Id, Secili = false,
                    Blok = t.Daire.Blok.Ad, Daire = t.Daire.DaireNo,
                    Tutar = t.Tutar, Odenen = t.OdenenTutar,
                    Vade = t.SonOdemeTarihi?.ToString("dd.MM.yyyy") ?? "-",
                    GecikmeTaz = t.GecikmeTazminati
                });
            }
        }
        catch (Exception ex) { _ = DialogService.ShowErrorAsync($"Gecikme listesi yüklenirken hata: {ex.Message}"); }
    }

    [RelayCommand] private void TopluTahakkuk() => RequestTopluTahakkukDialog?.Invoke(this, EventArgs.Empty);
    [RelayCommand] private void TahsilatGir() => RequestTahsilatGirDialog?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private async Task GecikmeTazminatiUygulaTabAsync()
    {
        try
        {
            var oran = GetFaiz();
            var svc = new AidatService();
            int count = 0;
            using var db = new AppDbContext();
            int ay = AyIndex + 1;
            var list = db.AidatTahakkuklar
                .Where(t => t.Yil == Yil && t.Ay == ay && t.Durum != TahakkukDurumu.Odenmis)
                .Where(t => t.SonOdemeTarihi != null && t.SonOdemeTarihi < DateTime.Now).ToList();
            foreach (var t in list)
            {
                var r = svc.GecikmeTazminatiHesapla(t.Id, oran);
                if (!r.Contains("Henüz") && !r.Contains("bulunamadı")) count++;
            }
            await DialogService.ShowInfoAsync($"{count} tahakkuka gecikme tazminatı uygulandı.");
            LoadTahakkuklar();
        }
        catch (Exception ex) { await DialogService.ShowErrorAsync($"Hata: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task GecikmeTumuneUygulaAsync()
    {
        try
        {
            var oran = GetFaiz();
            var svc = new AidatService();
            int count = 0;
            foreach (var row in GecikmeList)
            {
                var r = svc.GecikmeTazminatiHesapla(row.Id, oran);
                if (!r.Contains("Henüz") && !r.Contains("bulunamadı")) count++;
            }
            await DialogService.ShowInfoAsync($"{count} tahakkuka gecikme tazminatı uygulandı.");
            LoadGecikme();
        }
        catch (Exception ex) { await DialogService.ShowErrorAsync($"Hata: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task GecikmeSecilenlereUygulaAsync()
    {
        try
        {
            var oran = GetFaiz();
            var svc = new AidatService();
            int count = 0;
            foreach (var row in GecikmeList.Where(x => x.Secili))
            {
                var r = svc.GecikmeTazminatiHesapla(row.Id, oran);
                if (!r.Contains("Henüz") && !r.Contains("bulunamadı")) count++;
            }
            await DialogService.ShowInfoAsync($"{count} tahakkuka gecikme tazminatı uygulandı.");
            LoadGecikme();
        }
        catch (Exception ex) { await DialogService.ShowErrorAsync($"Hata: {ex.Message}"); }
    }

    [RelayCommand] private void EkstrePdf() => RequestSaveFile?.Invoke(this, "pdf");
    [RelayCommand] private void EkstreExcel() => RequestSaveFile?.Invoke(this, "excel");

    private decimal GetFaiz()
    {
        try
        {
            using var db = new AppDbContext();
            var a = db.SiteAyarlar.FirstOrDefault(x => x.Anahtar == "GecikmeFaizOrani");
            if (a != null && decimal.TryParse(a.Deger, out var o)) return o;
        }
        catch { }
        return 2m;
    }
}

public partial class TahakkukRow : ObservableObject
{
    public int Id { get; set; }
    public string Blok { get; set; } = "";
    public string DaireNo { get; set; } = "";
    public string Sakin { get; set; } = "";
    public string AidatTipi { get; set; } = "";
    public decimal Tutar { get; set; }
    public decimal Odenen { get; set; }
    public decimal GecikmeTaz { get; set; }
    public string Durum { get; set; } = "";
}

public partial class GecikmeRow : ObservableObject
{
    public int Id { get; set; }
    [ObservableProperty] private bool _secili;
    public string Blok { get; set; } = "";
    public string Daire { get; set; } = "";
    public decimal Tutar { get; set; }
    public decimal Odenen { get; set; }
    public string Vade { get; set; } = "";
    public decimal GecikmeTaz { get; set; }
}

public class DaireItem
{
    public int Id { get; set; }
    public string Display { get; set; } = "";
    public override string ToString() => Display;
}
