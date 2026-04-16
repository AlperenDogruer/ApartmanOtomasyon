using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SiteYonetim.UI.Avalonia.Views.Dialogs;

public partial class AidatTipiDialog : Window
{
    public AidatTipiDialog() { InitializeComponent(); }
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
