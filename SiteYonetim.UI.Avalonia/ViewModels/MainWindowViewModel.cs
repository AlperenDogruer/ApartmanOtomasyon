using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiteYonetim.Business.Services;
using SiteYonetim.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteYonetim.UI.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _siteAdi = "Site Yönetim";
    [ObservableProperty] private string _tarih = DateTime.Now.ToString("dd MMMM yyyy, dddd");
    [ObservableProperty] private string _kasaText = "Kasa: ₺0,00";
    [ObservableProperty] private IBrush _kasaBrush = Brushes.White;
    [ObservableProperty] private string _borcluText = "Bu ay borçlu: 0 daire";
    [ObservableProperty] private ViewModelBase? _currentView;
    [ObservableProperty] private MenuItemVm? _activeMenu;

    public List<MenuItemVm> MenuItems { get; }

    public MainWindowViewModel()
    {
        MenuItems = new List<MenuItemVm>
        {
            new("🏠", "Dashboard", () => new DashboardViewModel()),
            new("🏢", "Daire & Sakin", () => new DaireViewModel()),
            new("👤", "Sakinler", () => new SakinViewModel()),
            new("💰", "Aidat İşlemleri", () => new AidatViewModel()),
            new("📊", "Gelir & Gider", () => new GelirGiderViewModel()),
            new("📋", "Raporlar", () => new RaporlarViewModel()),
            new("⚙️", "Ayarlar", () => new AyarlarViewModel())
        };

        LoadSiteAdi();
        UpdateStatusBar();
        SelectMenu(MenuItems[0]);
    }

    [RelayCommand]
    public void SelectMenu(MenuItemVm item)
    {
        if (ActiveMenu != null) ActiveMenu.IsActive = false;
        item.IsActive = true;
        ActiveMenu = item;
        CurrentView = item.CreateViewModel();
        UpdateStatusBar();
    }

    private void LoadSiteAdi()
    {
        try
        {
            using var db = new AppDbContext();
            SiteAdi = db.SiteAyarlar.FirstOrDefault(a => a.Anahtar == "SiteAdi")?.Deger ?? "Site Yönetim";
        }
        catch { }
    }

    public void UpdateStatusBar()
    {
        try
        {
            var service = new GelirGiderService();
            var now = DateTime.Now;
            var (_, _, net) = service.KasaDurumu(new DateTime(2000, 1, 1), DateTime.Now);
            var ozet = service.AylikOzet(now.Year, now.Month);

            KasaText = $"Kasa Bakiyesi: {net:₺#,##0.00}";
            KasaBrush = net >= 0
                ? new SolidColorBrush(Color.FromRgb(39, 174, 96))
                : new SolidColorBrush(Color.FromRgb(231, 76, 60));
            BorcluText = $"Bu ay borçlu: {ozet.BorcluDaireSayisi} daire";
        }
        catch { }
    }
}

public partial class MenuItemVm : ObservableObject
{
    [ObservableProperty] private bool _isActive;
    public string Icon { get; }
    public string Text { get; }
    public Func<ViewModelBase> CreateViewModel { get; }

    public MenuItemVm(string icon, string text, Func<ViewModelBase> createViewModel)
    {
        Icon = icon;
        Text = text;
        CreateViewModel = createViewModel;
    }
}
