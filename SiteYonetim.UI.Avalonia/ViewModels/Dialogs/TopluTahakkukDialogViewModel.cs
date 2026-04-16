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

namespace SiteYonetim.UI.Avalonia.ViewModels.Dialogs;

public partial class TopluTahakkukDialogViewModel : ViewModelBase
{
    public ObservableCollection<string> AylarList { get; } = new(AidatViewModel.Aylar);
    public ObservableCollection<AidatTipi> AidatTipleri { get; } = new();

    [ObservableProperty] private int _yil = DateTime.Now.Year;
    [ObservableProperty] private int _ayIndex = DateTime.Now.Month - 1;
    [ObservableProperty] private AidatTipi? _selectedAidatTipi;
    [ObservableProperty] private decimal _birimTutar = 500;
    [ObservableProperty] private string _daireSayisiText = "";
    [ObservableProperty] private string _hata = "";

    public event EventHandler<bool>? RequestClose;

    public TopluTahakkukDialogViewModel()
    {
        using var db = new AppDbContext();
        foreach (var t in db.AidatTipleri.Where(a => a.Aktif).OrderBy(a => a.Ad).ToList())
            AidatTipleri.Add(t);
        if (AidatTipleri.Count > 0) SelectedAidatTipi = AidatTipleri[0];

        int c = db.Daireler.Count();
        DaireSayisiText = $"Kaç daire için tahakkuk oluşturulacak: {c}";
    }

    [RelayCommand]
    private async Task OlusturAsync()
    {
        if (SelectedAidatTipi == null) { Hata = "Lütfen aidat tipini seçiniz."; return; }
        try
        {
            var svc = new AidatService();
            var (count, msg) = svc.TopluTahakkukOlustur(Yil, AyIndex + 1, SelectedAidatTipi.Id, BirimTutar);
            if (count > 0) { await DialogService.ShowInfoAsync(msg); RequestClose?.Invoke(this, true); }
            else { await DialogService.ShowWarningAsync(msg); }
        }
        catch (Exception ex) { Hata = ex.Message; }
    }

    [RelayCommand] private void Iptal() => RequestClose?.Invoke(this, false);
}
