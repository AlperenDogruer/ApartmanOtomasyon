using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SiteYonetim.UI.Avalonia.ViewModels;
using SiteYonetim.UI.Avalonia.ViewModels.Dialogs;
using SiteYonetim.UI.Avalonia.Views.Dialogs;

namespace SiteYonetim.UI.Avalonia.Views;

public partial class SakinView : UserControl
{
    public SakinView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Hook();
    }
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    private Window? GetWindow() =>
        global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d ? d.MainWindow : null;

    private void Hook()
    {
        if (DataContext is not SakinViewModel vm) return;
        vm.RequestEditDialog += async (_, existing) =>
        {
            var dlgVm = new SakinEkleDialogViewModel(existing);
            var dlg = new SakinEkleDialog { DataContext = dlgVm };
            bool ok = false;
            dlgVm.RequestClose += (_, r) => { ok = r; dlg.Close(); };
            var owner = GetWindow();
            if (owner != null) await dlg.ShowDialog(owner);
            if (ok && dlgVm.SelectedDaire != null)
                vm.OnSave(existing, dlgVm.SelectedDaire.Id, dlgVm.Ad.Trim(), dlgVm.Soyad.Trim(),
                    Null(dlgVm.TcKimlik), Null(dlgVm.Telefon), Null(dlgVm.Email),
                    dlgVm.SelectedTip, dlgVm.GirisTarihi);
        };
        vm.RequestCikisDialog += async (_, sakin) =>
        {
            var dlgVm = new CikisTarihiDialogViewModel(sakin.AdSoyad);
            var dlg = new CikisTarihiDialog { DataContext = dlgVm };
            bool ok = false;
            dlgVm.RequestClose += (_, r) => { ok = r; dlg.Close(); };
            var owner = GetWindow();
            if (owner != null) await dlg.ShowDialog(owner);
            if (ok) vm.OnCikis(sakin.Id, dlgVm.CikisTarihi);
        };
    }

    private static string? Null(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
