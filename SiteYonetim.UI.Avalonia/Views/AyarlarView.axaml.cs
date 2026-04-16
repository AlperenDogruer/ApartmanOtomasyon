using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using SiteYonetim.UI.Avalonia.ViewModels;
using SiteYonetim.UI.Avalonia.ViewModels.Dialogs;
using SiteYonetim.UI.Avalonia.Views.Dialogs;
using System.Linq;

namespace SiteYonetim.UI.Avalonia.Views;

public partial class AyarlarView : UserControl
{
    public AyarlarView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Hook();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void Hook()
    {
        if (DataContext is not AyarlarViewModel vm) return;

        vm.RequestAidatTipiDialog += async (_, existing) =>
        {
            var dlgVm = new AidatTipiDialogViewModel(existing?.Ad, existing?.Aciklama);
            var dlg = new AidatTipiDialog { DataContext = dlgVm };
            bool ok = false;
            dlgVm.RequestClose += (_, r) => { ok = r; dlg.Close(); };
            var owner = GetWindow();
            if (owner != null) await dlg.ShowDialog(owner);
            if (ok) vm.SaveAidatTipi(existing, dlgVm.Ad.Trim(),
                string.IsNullOrWhiteSpace(dlgVm.Aciklama) ? null : dlgVm.Aciklama.Trim());
        };

        vm.RequestBackupFolder += async (_, _) =>
        {
            var top = TopLevel.GetTopLevel(this);
            if (top == null) return;
            var folders = await top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Yedek klasörünü seçin",
                AllowMultiple = false
            });
            var folder = folders.FirstOrDefault();
            if (folder != null) await vm.PerformBackupAsync(folder.Path.LocalPath);
        };
    }

    private Window? GetWindow()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
            return d.MainWindow;
        return null;
    }
}
