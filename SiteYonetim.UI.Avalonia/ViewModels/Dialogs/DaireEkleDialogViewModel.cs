using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiteYonetim.Core.Enums;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SiteYonetim.UI.Avalonia.ViewModels.Dialogs;

public partial class DaireEkleDialogViewModel : ViewModelBase
{
    [ObservableProperty] private string _baslik = "Yeni Daire Ekle";
    [ObservableProperty] private string _hata = "";
    [ObservableProperty] private Blok? _selectedBlok;
    [ObservableProperty] private string _daireNo = "";
    [ObservableProperty] private int _kat = 1;
    [ObservableProperty] private decimal _arsaPayi = 100;
    [ObservableProperty] private DaireTipi _selectedTip = DaireTipi.Daire;

    public ObservableCollection<Blok> Bloklar { get; } = new();
    public ObservableCollection<DaireTipi> Tipler { get; } = new(Enum.GetValues<DaireTipi>());

    public Daire? Existing { get; }
    public event EventHandler<bool>? RequestClose;

    public DaireEkleDialogViewModel(Daire? existing = null)
    {
        Existing = existing;
        using (var db = new AppDbContext())
        {
            foreach (var b in db.Bloklar.OrderBy(x => x.Ad).ToList()) Bloklar.Add(b);
        }

        if (existing != null)
        {
            Baslik = "Daire Düzenle";
            SelectedBlok = Bloklar.FirstOrDefault(b => b.Id == existing.BlokId);
            DaireNo = existing.DaireNo;
            Kat = existing.Kat;
            ArsaPayi = (decimal)existing.ArsakPayi;
            SelectedTip = existing.Tip;
        }
        else if (Bloklar.Count > 0) SelectedBlok = Bloklar[0];
    }

    [RelayCommand]
    private void Kaydet()
    {
        if (SelectedBlok == null) { Hata = "Lütfen bir blok seçin."; return; }
        if (string.IsNullOrWhiteSpace(DaireNo)) { Hata = "Daire numarası boş olamaz."; return; }
        RequestClose?.Invoke(this, true);
    }

    [RelayCommand] private void Iptal() => RequestClose?.Invoke(this, false);
}
