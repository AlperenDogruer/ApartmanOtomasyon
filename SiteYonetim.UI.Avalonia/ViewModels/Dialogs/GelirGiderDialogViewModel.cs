using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiteYonetim.Core.Enums;
using System;
using System.Collections.ObjectModel;

namespace SiteYonetim.UI.Avalonia.ViewModels.Dialogs;

public partial class GelirGiderDialogViewModel : ViewModelBase
{
    [ObservableProperty] private string _baslik;
    [ObservableProperty] private string _aciklama = "";
    [ObservableProperty] private decimal _tutar;
    [ObservableProperty] private DateTime _tarih = DateTime.Today;
    [ObservableProperty] private string? _kategori;
    [ObservableProperty] private OdemeTipi _odemeTipi = OdemeTipi.Nakit;
    [ObservableProperty] private string _belgeNo = "";
    [ObservableProperty] private string _tedarikci = "";
    [ObservableProperty] private bool _tedarikciGoster;
    [ObservableProperty] private string _hata = "";

    public ObservableCollection<string> Kategoriler { get; }
    public ObservableCollection<OdemeTipi> OdemeTipleri { get; } = new(Enum.GetValues<OdemeTipi>());

    public event EventHandler<bool>? RequestClose;

    public GelirGiderDialogViewModel(string baslik, string[] kategoriler, bool tedarikciGoster,
        string? aciklama = null, decimal? tutar = null, DateTime? tarih = null,
        string? kategori = null, OdemeTipi? odemeTipi = null, string? belgeNo = null, string? tedarikci = null)
    {
        Baslik = baslik;
        Kategoriler = new ObservableCollection<string>(kategoriler);
        TedarikciGoster = tedarikciGoster;
        Aciklama = aciklama ?? "";
        Tutar = tutar ?? 0;
        Tarih = tarih ?? DateTime.Today;
        Kategori = kategori ?? (kategoriler.Length > 0 ? kategoriler[0] : null);
        OdemeTipi = odemeTipi ?? OdemeTipi.Nakit;
        BelgeNo = belgeNo ?? "";
        Tedarikci = tedarikci ?? "";
    }

    [RelayCommand]
    private void Kaydet()
    {
        if (string.IsNullOrWhiteSpace(Aciklama)) { Hata = "Açıklama boş olamaz."; return; }
        RequestClose?.Invoke(this, true);
    }

    [RelayCommand] private void Iptal() => RequestClose?.Invoke(this, false);
}
