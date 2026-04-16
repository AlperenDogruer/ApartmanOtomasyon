using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SiteYonetim.Core.Enums;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SiteYonetim.UI.Avalonia.ViewModels.Dialogs;

public partial class SakinEkleDialogViewModel : ViewModelBase
{
    [ObservableProperty] private string _baslik = "Yeni Sakin Ekle";
    [ObservableProperty] private string _hata = "";
    [ObservableProperty] private string _ad = "";
    [ObservableProperty] private string _soyad = "";
    [ObservableProperty] private string _tcKimlik = "";
    [ObservableProperty] private string _telefon = "";
    [ObservableProperty] private string _email = "";
    [ObservableProperty] private DaireItem? _selectedDaire;
    [ObservableProperty] private SakinTipi _selectedTip = SakinTipi.Malik;
    [ObservableProperty] private DateTime _girisTarihi = DateTime.Today;

    public ObservableCollection<DaireItem> Daireler { get; } = new();
    public ObservableCollection<SakinTipi> Tipler { get; } = new(Enum.GetValues<SakinTipi>());

    public Sakin? Existing { get; }
    public event EventHandler<bool>? RequestClose;

    public SakinEkleDialogViewModel(Sakin? existing = null, int? preSelectedDaireId = null)
    {
        Existing = existing;
        using (var db = new AppDbContext())
        {
            foreach (var d in db.Daireler.Include(x => x.Blok).OrderBy(x => x.Blok.Ad).ThenBy(x => x.DaireNo).ToList())
                Daireler.Add(new DaireItem { Id = d.Id, Display = $"{d.Blok.Ad} - {d.DaireNo}" });
        }

        if (existing != null)
        {
            Baslik = "Sakin Düzenle";
            Ad = existing.Ad; Soyad = existing.Soyad;
            TcKimlik = existing.TcKimlik ?? ""; Telefon = existing.Telefon ?? "";
            Email = existing.Email ?? "";
            SelectedDaire = Daireler.FirstOrDefault(d => d.Id == existing.DaireId);
            SelectedTip = existing.Tip;
            GirisTarihi = existing.GirisTarihi;
        }
        else
        {
            if (preSelectedDaireId.HasValue)
                SelectedDaire = Daireler.FirstOrDefault(d => d.Id == preSelectedDaireId.Value);
            else if (Daireler.Count > 0) SelectedDaire = Daireler[0];
        }
    }

    [RelayCommand]
    private void Kaydet()
    {
        if (string.IsNullOrWhiteSpace(Ad)) { Hata = "Lütfen ad giriniz."; return; }
        if (string.IsNullOrWhiteSpace(Soyad)) { Hata = "Lütfen soyad giriniz."; return; }
        if (SelectedDaire == null) { Hata = "Lütfen bir daire seçin."; return; }
        RequestClose?.Invoke(this, true);
    }

    [RelayCommand] private void Iptal() => RequestClose?.Invoke(this, false);
}
