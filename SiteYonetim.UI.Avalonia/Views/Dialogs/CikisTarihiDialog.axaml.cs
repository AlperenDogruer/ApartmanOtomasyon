using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SiteYonetim.UI.Avalonia.Views.Dialogs;

public partial class CikisTarihiDialog : Window
{
    public CikisTarihiDialog() { InitializeComponent(); }
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
