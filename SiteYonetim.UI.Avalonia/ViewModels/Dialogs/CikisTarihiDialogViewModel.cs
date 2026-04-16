using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace SiteYonetim.UI.Avalonia.ViewModels.Dialogs;

public partial class CikisTarihiDialogViewModel : ViewModelBase
{
    [ObservableProperty] private string _sakinAdSoyad;
    [ObservableProperty] private DateTime _cikisTarihi = DateTime.Today;

    public event EventHandler<bool>? RequestClose;

    public CikisTarihiDialogViewModel(string sakinAdSoyad)
    {
        _sakinAdSoyad = sakinAdSoyad;
    }

    [RelayCommand] private void Onayla() => RequestClose?.Invoke(this, true);
    [RelayCommand] private void Iptal() => RequestClose?.Invoke(this, false);
}
