using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiteYonetim.Business.Services;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;
using SiteYonetim.UI.Avalonia.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SiteYonetim.UI.Avalonia.ViewModels;

public partial class GelirGiderViewModel : ViewModelBase
{
    public static readonly string[] GelirKategorileri = { "Tümü", "Aidat", "Kira", "Bağış", "Diğer" };
    public static readonly string[] GiderKategorileri = { "Tümü", "Temizlik", "Güvenlik", "Bakım", "Elektrik", "Su", "Doğalgaz", "Personel", "Sigorta", "Diğer" };

    [ObservableProperty] private DateTime _gelirBas = new(DateTime.Now.Year, DateTime.Now.Month, 1);
    [ObservableProperty] private DateTime _gelirBit = DateTime.Now;
    [ObservableProperty] private string _gelirKategori = "Tümü";
    [ObservableProperty] private string _gelirToplamText = "Toplam: ₺0,00";
    [ObservableProperty] private GelirKalemi? _selectedGelir;

    [ObservableProperty] private DateTime _giderBas = new(DateTime.Now.Year, DateTime.Now.Month, 1);
    [ObservableProperty] private DateTime _giderBit = DateTime.Now;
    [ObservableProperty] private string _giderKategori = "Tümü";
    [ObservableProperty] private string _giderToplamText = "Toplam: ₺0,00";
    [ObservableProperty] private GiderKalemi? _selectedGider;

    [ObservableProperty] private DateTime _kasaBas = new(DateTime.Now.Year, 1, 1);
    [ObservableProperty] private DateTime _kasaBit = DateTime.Now;
    [ObservableProperty] private string _toplamGelirText = "₺0,00";
    [ObservableProperty] private string _toplamGiderText = "₺0,00";
    [ObservableProperty] private string _netBakiyeText = "₺0,00";
    [ObservableProperty] private IBrush _netBrush = new SolidColorBrush(Color.FromRgb(30, 58, 95));

    public ObservableCollection<GelirKalemi> Gelirler { get; } = new();
    public ObservableCollection<GiderKalemi> Giderler { get; } = new();
    public ObservableCollection<KategoriOzet> KategoriOzetleri { get; } = new();
    public ObservableCollection<string> GelirKategoriList { get; } = new(GelirKategorileri);
    public ObservableCollection<string> GiderKategoriList { get; } = new(GiderKategorileri);

    public event EventHandler<GelirKalemi?>? RequestGelirDialog;
    public event EventHandler<GiderKalemi?>? RequestGiderDialog;

    public GelirGiderViewModel()
    {
        LoadGelirler(); LoadGiderler();
    }

    [RelayCommand]
    public void LoadGelirler()
    {
        Gelirler.Clear();
        try
        {
            var svc = new GelirGiderService();
            var list = svc.GelirListele(GelirBas.Date, GelirBit.Date, GelirKategori);
            decimal toplam = 0;
            foreach (var g in list) { Gelirler.Add(g); toplam += g.Tutar; }
            GelirToplamText = $"Toplam: {toplam:₺#,##0.00}";
        }
        catch (Exception ex) { _ = DialogService.ShowErrorAsync($"Gelirler yüklenirken hata: {ex.Message}"); }
    }

    [RelayCommand]
    public void LoadGiderler()
    {
        Giderler.Clear();
        try
        {
            var svc = new GelirGiderService();
            var list = svc.GiderListele(GiderBas.Date, GiderBit.Date, GiderKategori);
            decimal toplam = 0;
            foreach (var g in list) { Giderler.Add(g); toplam += g.Tutar; }
            GiderToplamText = $"Toplam: {toplam:₺#,##0.00}";
        }
        catch (Exception ex) { _ = DialogService.ShowErrorAsync($"Giderler yüklenirken hata: {ex.Message}"); }
    }

    [RelayCommand]
    public void LoadKasaOzeti()
    {
        try
        {
            var svc = new GelirGiderService();
            var (g, gi, n) = svc.KasaDurumu(KasaBas.Date, KasaBit.Date);
            ToplamGelirText = g.ToString("₺#,##0.00");
            ToplamGiderText = gi.ToString("₺#,##0.00");
            NetBakiyeText = n.ToString("₺#,##0.00");
            NetBrush = n >= 0 ? new SolidColorBrush(Color.FromRgb(39, 174, 96))
                              : new SolidColorBrush(Color.FromRgb(231, 76, 60));

            KategoriOzetleri.Clear();
            using var db = new AppDbContext();
            var gruplar = db.GiderKalemleri
                .Where(x => x.Tarih >= KasaBas.Date && x.Tarih <= KasaBit.Date)
                .ToList()
                .GroupBy(x => x.Kategori)
                .Select(gr => new KategoriOzet { Kategori = gr.Key, Tutar = gr.Sum(x => x.Tutar) })
                .OrderByDescending(x => x.Tutar);
            foreach (var item in gruplar) KategoriOzetleri.Add(item);
        }
        catch (Exception ex) { _ = DialogService.ShowErrorAsync($"Kasa özeti hatası: {ex.Message}"); }
    }

    [RelayCommand] private void GelirEkle() => RequestGelirDialog?.Invoke(this, null);
    [RelayCommand]
    private async Task GelirDuzenleAsync()
    {
        if (SelectedGelir == null) { await DialogService.ShowWarningAsync("Lütfen bir kayıt seçin."); return; }
        RequestGelirDialog?.Invoke(this, SelectedGelir);
    }
    [RelayCommand]
    private async Task GelirSilAsync()
    {
        if (SelectedGelir == null) return;
        if (!await DialogService.ConfirmAsync("Bu gelir kaydı silinecek. Emin misiniz?")) return;
        new GelirGiderService().GelirSil(SelectedGelir.Id);
        LoadGelirler();
    }

    [RelayCommand] private void GiderEkle() => RequestGiderDialog?.Invoke(this, null);
    [RelayCommand]
    private async Task GiderDuzenleAsync()
    {
        if (SelectedGider == null) { await DialogService.ShowWarningAsync("Lütfen bir kayıt seçin."); return; }
        RequestGiderDialog?.Invoke(this, SelectedGider);
    }
    [RelayCommand]
    private async Task GiderSilAsync()
    {
        if (SelectedGider == null) return;
        if (!await DialogService.ConfirmAsync("Bu gider kaydı silinecek. Emin misiniz?")) return;
        new GelirGiderService().GiderSil(SelectedGider.Id);
        LoadGiderler();
    }
}

public class KategoriOzet
{
    public string Kategori { get; set; } = "";
    public decimal Tutar { get; set; }
}
