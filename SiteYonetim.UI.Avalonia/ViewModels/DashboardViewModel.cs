using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using SiteYonetim.Business.Services;
using SiteYonetim.Core.DTOs;
using System;
using System.Collections.ObjectModel;

namespace SiteYonetim.UI.Avalonia.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    [ObservableProperty] private string _tahakkukText = "₺0,00";
    [ObservableProperty] private string _tahsilatText = "₺0,00";
    [ObservableProperty] private string _borcluCount = "0";
    [ObservableProperty] private string _kasaText = "₺0,00";
    [ObservableProperty] private IBrush _kasaBrush = new SolidColorBrush(Color.FromRgb(30, 58, 95));

    public ObservableCollection<BorcluDTO> Borclular { get; } = new();

    public DashboardViewModel()
    {
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            var gelirGiderService = new GelirGiderService();
            var aidatService = new AidatService();
            var now = DateTime.Now;

            var ozet = gelirGiderService.AylikOzet(now.Year, now.Month);
            TahakkukText = ozet.ToplamTahakkuk.ToString("₺#,##0.00");
            TahsilatText = ozet.AidatTahsilati.ToString("₺#,##0.00");
            BorcluCount = ozet.BorcluDaireSayisi.ToString();

            var (_, _, net) = gelirGiderService.KasaDurumu(new DateTime(2000, 1, 1), DateTime.Now);
            KasaText = net.ToString("₺#,##0.00");
            KasaBrush = net >= 0
                ? new SolidColorBrush(Color.FromRgb(39, 174, 96))
                : new SolidColorBrush(Color.FromRgb(231, 76, 60));

            Borclular.Clear();
            foreach (var b in aidatService.AylikBorcluListesi(now.Year, now.Month))
                Borclular.Add(b);
        }
        catch { }
    }
}
