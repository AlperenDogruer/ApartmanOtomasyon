using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SiteYonetim.Business.Services;
using SiteYonetim.Data.Entities;
using SiteYonetim.UI.Avalonia.ViewModels;
using SiteYonetim.UI.Avalonia.ViewModels.Dialogs;
using SiteYonetim.UI.Avalonia.Views.Dialogs;

namespace SiteYonetim.UI.Avalonia.Views;

public partial class GelirGiderView : UserControl
{
    public GelirGiderView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Hook();
    }
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    private Window? GetWindow() =>
        global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d ? d.MainWindow : null;

    private void Hook()
    {
        if (DataContext is not GelirGiderViewModel vm) return;

        vm.RequestGelirDialog += async (_, existing) =>
        {
            var cats = new[] { "Aidat", "Kira", "Bağış", "Diğer" };
            var dlgVm = new GelirGiderDialogViewModel(
                existing == null ? "Gelir Ekle" : "Gelir Düzenle",
                cats, false,
                existing?.Aciklama, existing?.Tutar, existing?.Tarih,
                existing?.Kategori, existing?.OdemeTipi, existing?.BelgeNo);
            var dlg = new GelirGiderDialog { DataContext = dlgVm };
            bool ok = false;
            dlgVm.RequestClose += (_, r) => { ok = r; dlg.Close(); };
            var owner = GetWindow();
            if (owner != null) await dlg.ShowDialog(owner);
            if (!ok) return;

            var svc = new GelirGiderService();
            if (existing == null)
            {
                svc.GelirKaydet(new GelirKalemi
                {
                    Aciklama = dlgVm.Aciklama.Trim(), Tutar = dlgVm.Tutar, Tarih = dlgVm.Tarih.Date,
                    Kategori = dlgVm.Kategori ?? "Diğer", OdemeTipi = dlgVm.OdemeTipi,
                    BelgeNo = string.IsNullOrWhiteSpace(dlgVm.BelgeNo) ? null : dlgVm.BelgeNo.Trim()
                });
            }
            else
            {
                existing.Aciklama = dlgVm.Aciklama.Trim();
                existing.Tutar = dlgVm.Tutar; existing.Tarih = dlgVm.Tarih.Date;
                existing.Kategori = dlgVm.Kategori ?? "Diğer"; existing.OdemeTipi = dlgVm.OdemeTipi;
                existing.BelgeNo = string.IsNullOrWhiteSpace(dlgVm.BelgeNo) ? null : dlgVm.BelgeNo.Trim();
                svc.GelirGuncelle(existing);
            }
            vm.LoadGelirler();
        };

        vm.RequestGiderDialog += async (_, existing) =>
        {
            var cats = new[] { "Temizlik", "Güvenlik", "Bakım", "Elektrik", "Su", "Doğalgaz", "Personel", "Sigorta", "Diğer" };
            var dlgVm = new GelirGiderDialogViewModel(
                existing == null ? "Gider Ekle" : "Gider Düzenle",
                cats, true,
                existing?.Aciklama, existing?.Tutar, existing?.Tarih,
                existing?.Kategori, existing?.OdemeTipi, existing?.BelgeNo, existing?.Tedarikci);
            var dlg = new GelirGiderDialog { DataContext = dlgVm };
            bool ok = false;
            dlgVm.RequestClose += (_, r) => { ok = r; dlg.Close(); };
            var owner = GetWindow();
            if (owner != null) await dlg.ShowDialog(owner);
            if (!ok) return;

            var svc = new GelirGiderService();
            if (existing == null)
            {
                svc.GiderKaydet(new GiderKalemi
                {
                    Aciklama = dlgVm.Aciklama.Trim(), Tutar = dlgVm.Tutar, Tarih = dlgVm.Tarih.Date,
                    Kategori = dlgVm.Kategori ?? "Diğer", OdemeTipi = dlgVm.OdemeTipi,
                    BelgeNo = string.IsNullOrWhiteSpace(dlgVm.BelgeNo) ? null : dlgVm.BelgeNo.Trim(),
                    Tedarikci = string.IsNullOrWhiteSpace(dlgVm.Tedarikci) ? null : dlgVm.Tedarikci.Trim()
                });
            }
            else
            {
                existing.Aciklama = dlgVm.Aciklama.Trim();
                existing.Tutar = dlgVm.Tutar; existing.Tarih = dlgVm.Tarih.Date;
                existing.Kategori = dlgVm.Kategori ?? "Diğer"; existing.OdemeTipi = dlgVm.OdemeTipi;
                existing.BelgeNo = string.IsNullOrWhiteSpace(dlgVm.BelgeNo) ? null : dlgVm.BelgeNo.Trim();
                existing.Tedarikci = string.IsNullOrWhiteSpace(dlgVm.Tedarikci) ? null : dlgVm.Tedarikci.Trim();
                svc.GiderGuncelle(existing);
            }
            vm.LoadGiderler();
        };
    }
}
