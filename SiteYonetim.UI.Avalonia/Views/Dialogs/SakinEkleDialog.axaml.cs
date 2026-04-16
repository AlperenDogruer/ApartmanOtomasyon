using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SiteYonetim.UI.Avalonia.Views.Dialogs;

public partial class SakinEkleDialog : Window
{
    public SakinEkleDialog() { InitializeComponent(); }
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
