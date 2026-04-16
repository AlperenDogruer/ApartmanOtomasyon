using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using SiteYonetim.Business.Services;
using SiteYonetim.Core.Helpers;
using SiteYonetim.UI.Avalonia.ViewModels;
using SiteYonetim.UI.Avalonia.ViewModels.Dialogs;
using SiteYonetim.UI.Avalonia.Views.Dialogs;
using System.Collections.Generic;
using System.Linq;

namespace SiteYonetim.UI.Avalonia.Views;

public partial class AidatView : UserControl
{
    public AidatView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Hook();
    }
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    private Window? GetWindow() =>
        global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d ? d.MainWindow : null;

    private void Hook()
    {
        if (DataContext is not AidatViewModel vm) return;
        vm.RequestTopluTahakkukDialog += async (_, _) =>
        {
            var dlgVm = new TopluTahakkukDialogViewModel();
            var dlg = new TopluTahakkukDialog { DataContext = dlgVm };
            dlgVm.RequestClose += (_, _) => dlg.Close();
            var owner = GetWindow();
            if (owner != null) await dlg.ShowDialog(owner);
            vm.LoadTahakkuklar();
        };
        vm.RequestTahsilatGirDialog += async (_, _) =>
        {
            var dlgVm = new TahsilatGirDialogViewModel();
            var dlg = new TahsilatGirDialog { DataContext = dlgVm };
            dlgVm.RequestClose += (_, _) => dlg.Close();
            var owner = GetWindow();
            if (owner != null) await dlg.ShowDialog(owner);
            vm.LoadTahakkuklar();
        };
        vm.RequestSaveFile += async (_, type) =>
        {
            if (vm.SelectedDaire == null) return;
            var top = TopLevel.GetTopLevel(this);
            if (top == null) return;
            var ext = type == "pdf" ? "pdf" : "xlsx";
            var file = await top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                SuggestedFileName = $"DaireEkstresi.{ext}",
                DefaultExtension = ext
            });
            if (file == null) return;
            var svc = new RaporService();
            var dt = svc.DaireEkstreRaporu(vm.SelectedDaire.Id, vm.EkstreBas, vm.EkstreBit);
            string[] headers = { "Dönem", "Aidat Tipi", "Tahakkuk", "Ödenen", "Kalan", "Gecikme Taz." };
            if (type == "pdf") ExportHelper.ExportToPdf(dt, headers, "Daire Ekstresi", file.Path.LocalPath);
            else ExportHelper.ExportToExcel(dt, headers, "Daire Ekstresi", file.Path.LocalPath);
            await Services.DialogService.ShowInfoAsync($"Dosya kaydedildi:\n{file.Path.LocalPath}");
        };
    }
}
