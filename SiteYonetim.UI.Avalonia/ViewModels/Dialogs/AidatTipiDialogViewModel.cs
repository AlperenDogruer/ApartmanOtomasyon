using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace SiteYonetim.UI.Avalonia.ViewModels.Dialogs;

public partial class AidatTipiDialogViewModel : ViewModelBase
{
    [ObservableProperty] private string _baslik;
    [ObservableProperty] private string _ad = "";
    [ObservableProperty] private string _aciklama = "";
    [ObservableProperty] private string _hata = "";

    public event EventHandler<bool>? RequestClose;

    public AidatTipiDialogViewModel(string? ad = null, string? aciklama = null)
    {
        Baslik = ad == null ? "Aidat Tipi Ekle" : "Aidat Tipi Düzenle";
        Ad = ad ?? "";
        Aciklama = aciklama ?? "";
    }

    [RelayCommand]
    private void Kaydet()
    {
        if (string.IsNullOrWhiteSpace(Ad)) { Hata = "Ad boş olamaz."; return; }
        RequestClose?.Invoke(this, true);
    }

    [RelayCommand] private void Iptal() => RequestClose?.Invoke(this, false);
}
