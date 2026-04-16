using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;
using SiteYonetim.UI.Avalonia.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SiteYonetim.UI.Avalonia.ViewModels;

public partial class AyarlarViewModel : ViewModelBase
{
    [ObservableProperty] private string _siteAdi = "";
    [ObservableProperty] private string _adres = "";
    [ObservableProperty] private string _yoneticiAdi = "";
    [ObservableProperty] private string _yoneticiTelefon = "";
    [ObservableProperty] private decimal _faizOrani = 2;
    [ObservableProperty] private int _sonOdemeGunu;
    [ObservableProperty] private string _sonYedekText = "Son yedek: -";
    [ObservableProperty] private AidatTipiRow? _selectedAidatTipi;

    public ObservableCollection<AidatTipiRow> AidatTipleri { get; } = new();

    public event EventHandler? RequestBackupFolder;
    public event EventHandler<AidatTipiRow?>? RequestAidatTipiDialog;

    public AyarlarViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            using var db = new AppDbContext();
            SiteAdi = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "SiteAdi")?.Deger ?? "";
            Adres = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "Adres")?.Deger ?? "";
            YoneticiAdi = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "YoneticiAdi")?.Deger ?? "";
            YoneticiTelefon = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "YoneticiTelefon")?.Deger ?? "";

            var faiz = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "GecikmeFaizOrani");
            if (faiz != null && decimal.TryParse(faiz.Deger, out var f)) FaizOrani = f;

            var gun = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "SonOdemeGunu");
            if (gun != null && int.TryParse(gun.Deger, out var g)) SonOdemeGunu = g;

            var yedek = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "SonYedekTarihi");
            SonYedekText = yedek != null ? $"Son yedek: {yedek.Deger}" : "Son yedek: -";

            LoadAidatTipleri();
        }
        catch (Exception ex)
        {
            _ = DialogService.ShowErrorAsync($"Ayarlar yüklenirken hata oluştu: {ex.Message}");
        }
    }

    private void LoadAidatTipleri()
    {
        AidatTipleri.Clear();
        using var db = new AppDbContext();
        foreach (var a in db.AidatTipleri.OrderBy(x => x.Ad).ToList())
            AidatTipleri.Add(new AidatTipiRow { Id = a.Id, Ad = a.Ad, Aciklama = a.Aciklama ?? "-", AktifText = a.Aktif ? "Evet" : "Hayır", Aktif = a.Aktif });
    }

    private void SaveAyar(string anahtar, string deger)
    {
        using var db = new AppDbContext();
        var ayar = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == anahtar);
        if (ayar != null) ayar.Deger = deger;
        else db.SiteAyarlar.Add(new SiteAyar { Anahtar = anahtar, Deger = deger });
        db.SaveChanges();
    }

    [RelayCommand]
    private async Task SiteBilgileriKaydetAsync()
    {
        try
        {
            SaveAyar("SiteAdi", SiteAdi.Trim());
            SaveAyar("Adres", Adres.Trim());
            SaveAyar("YoneticiAdi", YoneticiAdi.Trim());
            SaveAyar("YoneticiTelefon", YoneticiTelefon.Trim());
            await DialogService.ShowInfoAsync("Site bilgileri kaydedildi.");
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync($"Site bilgileri kaydedilirken hata: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task GecikmeKaydetAsync()
    {
        try
        {
            SaveAyar("GecikmeFaizOrani", FaizOrani.ToString());
            SaveAyar("SonOdemeGunu", SonOdemeGunu.ToString());
            await DialogService.ShowInfoAsync("Gecikme tazminatı ayarları kaydedildi.");
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync($"Ayarlar kaydedilirken hata: {ex.Message}");
        }
    }

    [RelayCommand]
    private void AidatTipiEkle() => RequestAidatTipiDialog?.Invoke(this, null);

    [RelayCommand]
    private void AidatTipiDuzenle()
    {
        if (SelectedAidatTipi != null)
            RequestAidatTipiDialog?.Invoke(this, SelectedAidatTipi);
    }

    [RelayCommand]
    private async Task AidatTipiAktifPasifAsync()
    {
        if (SelectedAidatTipi == null) return;
        try
        {
            using var db = new AppDbContext();
            var tip = db.AidatTipleri.Find(SelectedAidatTipi.Id);
            if (tip == null) return;
            tip.Aktif = !tip.Aktif;
            db.SaveChanges();
            LoadAidatTipleri();
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync($"Durum değiştirilirken hata: {ex.Message}");
        }
    }

    public void SaveAidatTipi(AidatTipiRow? existing, string ad, string? aciklama)
    {
        try
        {
            using var db = new AppDbContext();
            if (existing == null)
            {
                db.AidatTipleri.Add(new AidatTipi { Ad = ad, Aciklama = aciklama, Aktif = true });
            }
            else
            {
                var entity = db.AidatTipleri.Find(existing.Id);
                if (entity != null)
                {
                    entity.Ad = ad;
                    entity.Aciklama = aciklama;
                }
            }
            db.SaveChanges();
            LoadAidatTipleri();
        }
        catch (Exception ex)
        {
            _ = DialogService.ShowErrorAsync($"Aidat tipi kaydedilirken hata: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Yedekle() => RequestBackupFolder?.Invoke(this, EventArgs.Empty);

    public async Task PerformBackupAsync(string folder)
    {
        try
        {
            string dbFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SiteYonetim");
            string dbPath = Path.Combine(dbFolder, "siteYonetim.db");

            if (!File.Exists(dbPath))
            {
                await DialogService.ShowErrorAsync("Veritabanı dosyası bulunamadı.");
                return;
            }

            string backupName = $"siteYonetim_backup_{DateTime.Now:yyyyMMdd_HHmm}.db";
            string backupPath = Path.Combine(folder, backupName);
            File.Copy(dbPath, backupPath, overwrite: true);

            string yedekTarihi = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            SaveAyar("SonYedekTarihi", yedekTarihi);
            SonYedekText = $"Son yedek: {yedekTarihi}";

            await DialogService.ShowInfoAsync($"Veritabanı yedeklendi:\n{backupPath}");
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync($"Yedekleme sırasında hata: {ex.Message}");
        }
    }
}

public partial class AidatTipiRow : ObservableObject
{
    public int Id { get; set; }
    [ObservableProperty] private string _ad = "";
    [ObservableProperty] private string _aciklama = "";
    [ObservableProperty] private string _aktifText = "";
    public bool Aktif { get; set; }
}
