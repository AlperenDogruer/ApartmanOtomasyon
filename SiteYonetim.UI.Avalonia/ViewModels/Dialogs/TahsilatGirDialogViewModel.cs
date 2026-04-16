using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SiteYonetim.Business.Services;
using SiteYonetim.Core.Enums;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;
using SiteYonetim.UI.Avalonia.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SiteYonetim.UI.Avalonia.ViewModels.Dialogs;

public partial class TahsilatGirDialogViewModel : ViewModelBase
{
    public ObservableCollection<DaireItem> Daireler { get; } = new();
    public ObservableCollection<TahakkukSecimRow> AcikTahakkuklar { get; } = new();
    public ObservableCollection<OdemeTipi> OdemeTipleri { get; } = new(Enum.GetValues<OdemeTipi>());

    [ObservableProperty] private DaireItem? _selectedDaire;
    [ObservableProperty] private TahakkukSecimRow? _selectedTahakkuk;
    [ObservableProperty] private decimal _tutar;
    [ObservableProperty] private OdemeTipi _selectedOdemeTipi = OdemeTipi.Nakit;
    [ObservableProperty] private string _aciklama = "";
    [ObservableProperty] private string _hata = "";

    public event EventHandler<bool>? RequestClose;

    public TahsilatGirDialogViewModel()
    {
        using var db = new AppDbContext();
        foreach (var d in db.Daireler.Include(x => x.Blok)
                     .OrderBy(x => x.Blok.Ad).ThenBy(x => x.DaireNo).ToList())
            Daireler.Add(new DaireItem { Id = d.Id, Display = $"{d.Blok.Ad} - {d.DaireNo}" });
        if (Daireler.Count > 0) SelectedDaire = Daireler[0];
    }

    partial void OnSelectedDaireChanged(DaireItem? value) => LoadAcikTahakkuklar();
    partial void OnSelectedTahakkukChanged(TahakkukSecimRow? value)
    {
        if (value != null) Tutar = Math.Max(0, value.Kalan);
    }

    private void LoadAcikTahakkuklar()
    {
        AcikTahakkuklar.Clear();
        if (SelectedDaire == null) return;
        using var db = new AppDbContext();
        var list = db.AidatTahakkuklar.Include(t => t.AidatTipi)
            .Where(t => t.DaireId == SelectedDaire.Id && t.Durum != TahakkukDurumu.Odenmis)
            .OrderBy(t => t.Yil).ThenBy(t => t.Ay).ToList();
        foreach (var t in list)
        {
            AcikTahakkuklar.Add(new TahakkukSecimRow
            {
                Id = t.Id,
                Donem = $"{t.Yil}/{t.Ay:D2}",
                AidatTipi = t.AidatTipi.Ad,
                Tutar = t.Tutar, Odenen = t.OdenenTutar,
                Kalan = t.Tutar - t.OdenenTutar + t.GecikmeTazminati
            });
        }
    }

    [RelayCommand]
    private async Task KaydetAsync()
    {
        if (SelectedTahakkuk == null) { Hata = "Lütfen bir tahakkuk seçin."; return; }
        if (Tutar <= 0) { Hata = "Geçerli bir tutar girin."; return; }
        try
        {
            var svc = new AidatService();
            var msg = svc.TahsilatKaydet(SelectedTahakkuk.Id, Tutar, SelectedOdemeTipi,
                string.IsNullOrWhiteSpace(Aciklama) ? null : Aciklama.Trim());
            await DialogService.ShowInfoAsync(msg);
            RequestClose?.Invoke(this, true);
        }
        catch (Exception ex) { Hata = ex.Message; }
    }

    [RelayCommand] private void Iptal() => RequestClose?.Invoke(this, false);
}

public class TahakkukSecimRow
{
    public int Id { get; set; }
    public string Donem { get; set; } = "";
    public string AidatTipi { get; set; } = "";
    public decimal Tutar { get; set; }
    public decimal Odenen { get; set; }
    public decimal Kalan { get; set; }
}
