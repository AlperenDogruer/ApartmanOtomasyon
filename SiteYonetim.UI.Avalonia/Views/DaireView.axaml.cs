using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SiteYonetim.UI.Avalonia.ViewModels;
using SiteYonetim.UI.Avalonia.ViewModels.Dialogs;
using SiteYonetim.UI.Avalonia.Views.Dialogs;

namespace SiteYonetim.UI.Avalonia.Views;

public partial class DaireView : UserControl
{
    public DaireView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Hook();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private Window? GetWindow()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
            return d.MainWindow;
        return null;
    }

    private void Hook()
    {
        if (DataContext is not DaireViewModel vm) return;

        vm.RequestBlokDialog += async (_, existing) =>
        {
            var dlgVm = new BlokEkleDialogViewModel(existing);
            var dlg = new BlokEkleDialog { DataContext = dlgVm };
            bool ok = false;
            dlgVm.RequestClose += (_, r) => { ok = r; dlg.Close(); };
            var owner = GetWindow();
            if (owner != null) await dlg.ShowDialog(owner);
            if (ok) vm.OnBlokSaved(existing, dlgVm.Ad.Trim(),
                string.IsNullOrWhiteSpace(dlgVm.Aciklama) ? null : dlgVm.Aciklama.Trim());
        };

        vm.RequestDaireDialog += async (_, existing) =>
        {
            var dlgVm = new DaireEkleDialogViewModel(existing);
            var dlg = new DaireEkleDialog { DataContext = dlgVm };
            bool ok = false;
            dlgVm.RequestClose += (_, r) => { ok = r; dlg.Close(); };
            var owner = GetWindow();
            if (owner != null) await dlg.ShowDialog(owner);
            if (ok && dlgVm.SelectedBlok != null)
                vm.OnDaireSaved(existing, dlgVm.SelectedBlok.Id, dlgVm.DaireNo.Trim(),
                    dlgVm.Kat, (float)dlgVm.ArsaPayi, dlgVm.SelectedTip);
        };

        vm.RequestSakinDialog += async (_, daireId) =>
        {
            var dlgVm = new SakinEkleDialogViewModel(null, daireId);
            var dlg = new SakinEkleDialog { DataContext = dlgVm };
            bool ok = false;
            dlgVm.RequestClose += (_, r) => { ok = r; dlg.Close(); };
            var owner = GetWindow();
            if (owner != null) await dlg.ShowDialog(owner);
            if (ok && dlgVm.SelectedDaire != null)
                vm.OnSakinSaved(dlgVm.SelectedDaire.Id, dlgVm.Ad.Trim(), dlgVm.Soyad.Trim(),
                    Null(dlgVm.TcKimlik), Null(dlgVm.Telefon), Null(dlgVm.Email),
                    dlgVm.SelectedTip, dlgVm.GirisTarihi);
        };

        vm.RequestSakinCikisDialog += async (_, sakin) =>
        {
            var dlgVm = new CikisTarihiDialogViewModel(sakin.AdSoyad);
            var dlg = new CikisTarihiDialog { DataContext = dlgVm };
            bool ok = false;
            dlgVm.RequestClose += (_, r) => { ok = r; dlg.Close(); };
            var owner = GetWindow();
            if (owner != null) await dlg.ShowDialog(owner);
            if (ok) vm.OnSakinCikis(sakin.Id, dlgVm.CikisTarihi);
        };
    }

    private static string? Null(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
