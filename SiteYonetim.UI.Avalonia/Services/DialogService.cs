using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace SiteYonetim.UI.Avalonia.Services;

public static class DialogService
{
    public static global::Avalonia.Controls.Window? GetMainWindow()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }

    public static async Task ShowInfoAsync(string message, string title = "Bilgi")
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.Info);
        var owner = GetMainWindow();
        if (owner != null) await box.ShowWindowDialogAsync(owner);
        else await box.ShowAsync();
    }

    public static async Task ShowErrorAsync(string message, string title = "Hata")
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.Error);
        var owner = GetMainWindow();
        if (owner != null) await box.ShowWindowDialogAsync(owner);
        else await box.ShowAsync();
    }

    public static async Task ShowWarningAsync(string message, string title = "Uyarı")
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.Warning);
        var owner = GetMainWindow();
        if (owner != null) await box.ShowWindowDialogAsync(owner);
        else await box.ShowAsync();
    }

    public static async Task<bool> ConfirmAsync(string message, string title = "Onay")
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNo, Icon.Question);
        var owner = GetMainWindow();
        var result = owner != null ? await box.ShowWindowDialogAsync(owner) : await box.ShowAsync();
        return result == ButtonResult.Yes;
    }
}
