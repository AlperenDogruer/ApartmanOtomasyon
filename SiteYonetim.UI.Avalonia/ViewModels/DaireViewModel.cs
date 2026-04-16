using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SiteYonetim.Business.Services;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;
using SiteYonetim.UI.Avalonia.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SiteYonetim.UI.Avalonia.ViewModels;

public partial class DaireViewModel : ViewModelBase
{
    public ObservableCollection<BlokNode> Bloklar { get; } = new();
    public ObservableCollection<SakinRow> Sakinler { get; } = new();
    public ObservableCollection<EkstreRow> AidatGecmisi { get; } = new();

    [ObservableProperty] private object? _selectedNode;
    [ObservableProperty] private string _daireNoText = "-";
    [ObservableProperty] private string _katText = "-";
    [ObservableProperty] private string _blokText = "-";
    [ObservableProperty] private string _arsaPayiText = "-";
    [ObservableProperty] private string _tipText = "-";
    [ObservableProperty] private SakinRow? _selectedSakin;

    public event EventHandler<Blok?>? RequestBlokDialog;
    public event EventHandler<Daire?>? RequestDaireDialog;
    public event EventHandler<int>? RequestSakinDialog; // daireId
    public event EventHandler<SakinRow>? RequestSakinCikisDialog;

    public DaireViewModel()
    {
        LoadTree();
    }

    private void LoadTree()
    {
        Bloklar.Clear();
        try
        {
            using var db = new AppDbContext();
            foreach (var b in db.Bloklar.Include(x => x.Daireler).OrderBy(x => x.Ad).ToList())
            {
                var blokNode = new BlokNode { Blok = b, Ad = b.Ad };
                foreach (var d in b.Daireler.OrderBy(x => x.DaireNo))
                    blokNode.Children.Add(new DaireNode { Daire = d, Ad = $"Daire {d.DaireNo}" });
                Bloklar.Add(blokNode);
            }
        }
        catch (Exception ex) { _ = DialogService.ShowErrorAsync($"Ağaç yüklenirken hata: {ex.Message}"); }
    }

    partial void OnSelectedNodeChanged(object? value)
    {
        if (value is DaireNode dn) LoadDaire(dn.Daire);
        else ClearDaire();
    }

    private void LoadDaire(Daire daire)
    {
        try
        {
            using var db = new AppDbContext();
            var d = db.Daireler.Include(x => x.Blok).FirstOrDefault(x => x.Id == daire.Id);
            if (d == null) return;
            DaireNoText = d.DaireNo;
            KatText = d.Kat.ToString();
            BlokText = d.Blok.Ad;
            ArsaPayiText = d.ArsakPayi.ToString("0.##");
            TipText = d.Tip.ToString();
            LoadSakinler(d.Id);
            LoadAidatGecmisi(d.Id);
        }
        catch (Exception ex) { _ = DialogService.ShowErrorAsync($"Daire yüklenirken hata: {ex.Message}"); }
    }

    private void ClearDaire()
    {
        DaireNoText = KatText = BlokText = ArsaPayiText = TipText = "-";
        Sakinler.Clear();
        AidatGecmisi.Clear();
    }

    private void LoadSakinler(int daireId)
    {
        Sakinler.Clear();
        using var db = new AppDbContext();
        foreach (var s in db.Sakinler.Where(x => x.DaireId == daireId)
                     .OrderByDescending(x => x.Aktif).ThenBy(x => x.Ad).ToList())
        {
            Sakinler.Add(new SakinRow
            {
                Id = s.Id,
                AdSoyad = $"{s.Ad} {s.Soyad}",
                Tip = s.Tip.ToString(),
                Telefon = s.Telefon ?? "-",
                GirisTarihi = s.GirisTarihi.ToString("dd.MM.yyyy"),
                Durum = s.Aktif ? "Aktif" : "Pasif"
            });
        }
    }

    private void LoadAidatGecmisi(int daireId)
    {
        AidatGecmisi.Clear();
        try
        {
            var service = new AidatService();
            var bas = DateTime.Now.AddMonths(-12);
            foreach (var e in service.DaireEkstresi(daireId, bas, DateTime.Now))
            {
                AidatGecmisi.Add(new EkstreRow
                {
                    Donem = e.Donem,
                    AidatTipi = e.AidatTipi,
                    Tahakkuk = e.TahakkukTutar,
                    Odenen = e.OdenenTutar,
                    Kalan = e.KalanTutar,
                    GecikmeTaz = e.GecikmeTazminati,
                    OdemeTarihi = e.OdemeTarihi ?? "-"
                });
            }
        }
        catch { }
    }

    [RelayCommand] private void BlokEkle() => RequestBlokDialog?.Invoke(this, null);
    [RelayCommand] private void DaireEkle() => RequestDaireDialog?.Invoke(this, null);

    [RelayCommand]
    private void Duzenle()
    {
        if (SelectedNode is DaireNode dn) RequestDaireDialog?.Invoke(this, dn.Daire);
        else if (SelectedNode is BlokNode bn) RequestBlokDialog?.Invoke(this, bn.Blok);
    }

    [RelayCommand]
    private async Task SilAsync()
    {
        try
        {
            if (SelectedNode is DaireNode dn)
            {
                if (!await DialogService.ConfirmAsync($"Daire {dn.Daire.DaireNo} silinecek. Emin misiniz?")) return;
                using var db = new AppDbContext();
                var e = db.Daireler.Find(dn.Daire.Id);
                if (e != null) { db.Daireler.Remove(e); db.SaveChanges(); }
                LoadTree(); ClearDaire();
            }
            else if (SelectedNode is BlokNode bn)
            {
                if (!await DialogService.ConfirmAsync($"{bn.Blok.Ad} bloku ve tüm daireleri silinecek. Emin misiniz?")) return;
                using var db = new AppDbContext();
                var e = db.Bloklar.Include(b => b.Daireler).FirstOrDefault(b => b.Id == bn.Blok.Id);
                if (e != null) { db.Bloklar.Remove(e); db.SaveChanges(); }
                LoadTree(); ClearDaire();
            }
            else
            {
                await DialogService.ShowWarningAsync("Lütfen silmek için bir blok veya daire seçin.");
            }
        }
        catch (Exception ex) { await DialogService.ShowErrorAsync($"Silme sırasında hata: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task SakinEkleAsync()
    {
        if (SelectedNode is not DaireNode dn)
        {
            await DialogService.ShowWarningAsync("Lütfen önce bir daire seçin.");
            return;
        }
        RequestSakinDialog?.Invoke(this, dn.Daire.Id);
    }

    [RelayCommand]
    private async Task SakinCikisAsync()
    {
        if (SelectedSakin == null)
        {
            await DialogService.ShowWarningAsync("Lütfen bir sakin seçin.");
            return;
        }
        RequestSakinCikisDialog?.Invoke(this, SelectedSakin);
    }

    public void OnBlokSaved(Blok? existing, string ad, string? aciklama)
    {
        using var db = new AppDbContext();
        if (existing == null) db.Bloklar.Add(new Blok { Ad = ad, Aciklama = aciklama });
        else
        {
            var e = db.Bloklar.Find(existing.Id);
            if (e != null) { e.Ad = ad; e.Aciklama = aciklama; }
        }
        db.SaveChanges();
        LoadTree();
    }

    public void OnDaireSaved(Daire? existing, int blokId, string daireNo, int kat, float arsaPayi, SiteYonetim.Core.Enums.DaireTipi tip)
    {
        using var db = new AppDbContext();
        if (existing == null)
            db.Daireler.Add(new Daire { BlokId = blokId, DaireNo = daireNo, Kat = kat, ArsakPayi = arsaPayi, Tip = tip });
        else
        {
            var e = db.Daireler.Find(existing.Id);
            if (e != null) { e.BlokId = blokId; e.DaireNo = daireNo; e.Kat = kat; e.ArsakPayi = arsaPayi; e.Tip = tip; }
        }
        db.SaveChanges();
        LoadTree();
    }

    public void OnSakinSaved(int daireId, string ad, string soyad, string? tc, string? tel, string? email, SiteYonetim.Core.Enums.SakinTipi tip, DateTime giris)
    {
        using var db = new AppDbContext();
        db.Sakinler.Add(new Sakin
        {
            DaireId = daireId, Ad = ad, Soyad = soyad, TcKimlik = tc, Telefon = tel, Email = email,
            Tip = tip, GirisTarihi = giris, Aktif = true
        });
        db.SaveChanges();
        LoadSakinler(daireId);
    }

    public void OnSakinCikis(int sakinId, DateTime cikisTarihi)
    {
        using var db = new AppDbContext();
        var s = db.Sakinler.Find(sakinId);
        if (s != null) { s.Aktif = false; s.CikisTarihi = cikisTarihi; db.SaveChanges(); }
        if (SelectedNode is DaireNode dn) LoadSakinler(dn.Daire.Id);
    }
}

public class BlokNode
{
    public Blok Blok { get; set; } = null!;
    public string Ad { get; set; } = "";
    public ObservableCollection<DaireNode> Children { get; set; } = new();
}

public class DaireNode
{
    public Daire Daire { get; set; } = null!;
    public string Ad { get; set; } = "";
}

public class SakinRow
{
    public int Id { get; set; }
    public string AdSoyad { get; set; } = "";
    public string Tip { get; set; } = "";
    public string Telefon { get; set; } = "";
    public string GirisTarihi { get; set; } = "";
    public string Durum { get; set; } = "";
    public string? DaireNo { get; set; }
    public string? Blok { get; set; }
}

public class EkstreRow
{
    public string Donem { get; set; } = "";
    public string AidatTipi { get; set; } = "";
    public decimal Tahakkuk { get; set; }
    public decimal Odenen { get; set; }
    public decimal Kalan { get; set; }
    public decimal GecikmeTaz { get; set; }
    public string OdemeTarihi { get; set; } = "-";
}
