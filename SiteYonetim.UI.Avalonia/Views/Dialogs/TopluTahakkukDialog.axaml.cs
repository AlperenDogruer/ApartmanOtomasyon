using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SiteYonetim.UI.Avalonia.Views.Dialogs;

public partial class TopluTahakkukDialog : Window
{
    public TopluTahakkukDialog() { InitializeComponent(); }
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
