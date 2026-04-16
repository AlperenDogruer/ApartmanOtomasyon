using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiteYonetim.Core.Enums;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;
using System;

namespace SiteYonetim.UI.Avalonia.ViewModels;

public partial class SetupWizardViewModel : ViewModelBase
{
    [ObservableProperty] private string _siteAdi = "";
    [ObservableProperty] private string _yoneticiAdi = "";
    [ObservableProperty] private int _currentStep = 1;
    [ObservableProperty] private string _adimText = "Adım 1/2 - Site Bilgileri";
    [ObservableProperty] private bool _step1Visible = true;
    [ObservableProperty] private bool _step2Visible;
    [ObservableProperty] private bool _geriVisible;
    [ObservableProperty] private bool _ileriVisible = true;
    [ObservableProperty] private bool _tamamlaVisible;
    [ObservableProperty] private string _hataMesaji = "";

    public event EventHandler<bool>? RequestClose;

    [RelayCommand]
    private void Ileri()
    {
        if (string.IsNullOrWhiteSpace(SiteAdi))
        {
            HataMesaji = "Lütfen site adını giriniz.";
            return;
        }
        HataMesaji = "";
        CurrentStep = 2;
        Step1Visible = false;
        Step2Visible = true;
        GeriVisible = true;
        IleriVisible = false;
        TamamlaVisible = true;
        AdimText = "Adım 2/2 - Yönetici Bilgileri";
    }

    [RelayCommand]
    private void Geri()
    {
        HataMesaji = "";
        CurrentStep = 1;
        Step1Visible = true;
        Step2Visible = false;
        GeriVisible = false;
        IleriVisible = true;
        TamamlaVisible = false;
        AdimText = "Adım 1/2 - Site Bilgileri";
    }

    [RelayCommand]
    private void Tamamla()
    {
        if (string.IsNullOrWhiteSpace(YoneticiAdi))
        {
            HataMesaji = "Lütfen yönetici adını giriniz.";
            return;
        }

        try
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            db.SiteAyarlar.Add(new SiteAyar { Anahtar = "SiteAdi", Deger = SiteAdi.Trim() });
            db.SiteAyarlar.Add(new SiteAyar { Anahtar = "YoneticiAdi", Deger = YoneticiAdi.Trim() });
            db.SiteAyarlar.Add(new SiteAyar { Anahtar = "KurulumTamamlandi", Deger = "true" });

            var blokA = new Blok { Ad = "A Blok" };
            var blokB = new Blok { Ad = "B Blok" };
            db.Bloklar.AddRange(blokA, blokB);
            db.SaveChanges();

            for (int i = 1; i <= 3; i++)
            {
                db.Daireler.Add(new Daire
                {
                    BlokId = blokA.Id,
                    DaireNo = i.ToString(),
                    Kat = i,
                    ArsakPayi = 100,
                    Tip = DaireTipi.Daire
                });
                db.Daireler.Add(new Daire
                {
                    BlokId = blokB.Id,
                    DaireNo = i.ToString(),
                    Kat = i,
                    ArsakPayi = 100,
                    Tip = DaireTipi.Daire
                });
            }

            db.AidatTipleri.Add(new AidatTipi { Ad = "İşletme Aidatı", Aktif = true });
            db.SaveChanges();

            RequestClose?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            HataMesaji = $"Kurulum sırasında hata: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Iptal()
    {
        RequestClose?.Invoke(this, false);
    }
}
