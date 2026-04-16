using CommunityToolkit.Mvvm.ComponentModel;

namespace SiteYonetim.UI.Avalonia.ViewModels;

public partial class PlaceholderViewModel : ViewModelBase
{
    [ObservableProperty] private string _title;
    public string Message => $"{Title} ekranı yakında Mac sürümüne eklenecek.\n\nŞu an WinForms sürümünde mevcut — Avalonia'ya port etmek için sıradaki adım.";

    public PlaceholderViewModel(string title)
    {
        _title = title;
    }
}
