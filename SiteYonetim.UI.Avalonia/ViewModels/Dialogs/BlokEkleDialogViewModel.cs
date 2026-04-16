using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiteYonetim.Data.Entities;
using System;

namespace SiteYonetim.UI.Avalonia.ViewModels.Dialogs;

public partial class BlokEkleDialogViewModel : ViewModelBase
{
    [ObservableProperty] private string _ad = "";
    [ObservableProperty] private string _aciklama = "";
    [ObservableProperty] private string _baslik = "Yeni Blok Ekle";
    [ObservableProperty] private string _hata = "";

    public Blok? Existing { get; }
    public event EventHandler<bool>? RequestClose;

    public BlokEkleDialogViewModel(Blok? existing = null)
    {
        Existing = existing;
        if (existing != null)
        {
            Baslik = "Blok Düzenle";
            Ad = existing.Ad;
            Aciklama = existing.Aciklama ?? "";
        }
    }

    [RelayCommand]
    private void Kaydet()
    {
        if (string.IsNullOrWhiteSpace(Ad)) { Hata = "Lütfen blok adını giriniz."; return; }
        RequestClose?.Invoke(this, true);
    }

    [RelayCommand]
    private void Iptal() => RequestClose?.Invoke(this, false);
}
