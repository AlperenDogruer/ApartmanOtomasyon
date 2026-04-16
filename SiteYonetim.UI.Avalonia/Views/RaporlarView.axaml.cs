using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using SiteYonetim.UI.Avalonia.ViewModels;

namespace SiteYonetim.UI.Avalonia.Views;

public partial class RaporlarView : UserControl
{
    public RaporlarView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Hook();
    }
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void Hook()
    {
        if (DataContext is not RaporlarViewModel vm) return;
        vm.RequestSaveFile += async (_, type) =>
        {
            var top = TopLevel.GetTopLevel(this);
            if (top == null) return;
            var ext = type == "pdf" ? "pdf" : "xlsx";
            var file = await top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                SuggestedFileName = $"Rapor.{ext}", DefaultExtension = ext
            });
            if (file == null) return;
            await vm.SaveAsync(type, file.Path.LocalPath);
        };
    }
}
