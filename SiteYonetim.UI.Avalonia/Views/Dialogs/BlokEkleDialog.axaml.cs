using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SiteYonetim.UI.Avalonia.Views.Dialogs;

public partial class BlokEkleDialog : Window
{
    public BlokEkleDialog() { InitializeComponent(); }
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
