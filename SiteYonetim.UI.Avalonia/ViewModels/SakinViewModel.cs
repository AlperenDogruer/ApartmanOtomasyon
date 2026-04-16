using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SiteYonetim.Data;
using SiteYonetim.Data.Entities;
using SiteYonetim.UI.Avalonia.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SiteYonetim.UI.Avalonia.ViewModels;

public partial class SakinViewModel : ViewModelBase
{
    [ObservableProperty] private string _arama = "";
    [ObservableProperty] private SakinRow? _selectedSakin;

    public ObservableCollection<SakinRow> Sakinler { get; } = new();
    private List<Sakin> _all = new();

    public event EventHandler<Sakin?>? RequestEditDialog;
    public event EventHandler<SakinRow>? RequestCikisDialog;

    public SakinViewModel()
    {
        LoadData();
    }

    partial void OnAramaChanged(string value) => Filter();

    private void LoadData()
    {
        try
        {
            using var db = new AppDbContext();
            _all = db.Sakinler.Include(s => s.Daire).ThenInclude(d => d.Blok)
                .OrderByDescending(s => s.Aktif).ThenBy(s => s.Ad).ThenBy(s => s.Soyad)
                .ToList();
            Filter();
        }
        catch (Exception ex) { _ = DialogService.ShowErrorAsync($"Sakinler yüklenirken hata: {ex.Message}"); }
    }

    private void Filter()
    {
        Sakinler.Clear();
        var f = Arama.Trim().ToLowerInvariant();
        var list = string.IsNullOrEmpty(f) ? _all
            : _all.Where(s => $"{s.Ad} {s.Soyad}".ToLowerInvariant().Contains(f)
                           || s.Daire.DaireNo.ToLowerInvariant().Contains(f));
        foreach (var s in list)
        {
            Sakinler.Add(new SakinRow
            {
                Id = s.Id,
                AdSoyad = $"{s.Ad} {s.Soyad}",
                DaireNo = s.Daire.DaireNo,
                Blok = s.Daire.Blok.Ad,
                Tip = s.Tip.ToString(),
                Telefon = s.Telefon ?? "-",
                GirisTarihi = s.GirisTarihi.ToString("dd.MM.yyyy"),
                Durum = s.Aktif ? "Aktif" : "Pasif"
            });
        }
    }

    [RelayCommand] private void Ekle() => RequestEditDialog?.Invoke(this, null);

    [RelayCommand]
    private async Task DuzenleAsync()
    {
        if (SelectedSakin == null)
        {
            await DialogService.ShowWarningAsync("Lütfen bir sakin seçin.");
            return;
        }
        using var db = new AppDbContext();
        var s = db.Sakinler.Find(SelectedSakin.Id);
        if (s != null) RequestEditDialog?.Invoke(this, s);
    }

    [RelayCommand]
    private async Task CikisAsync()
    {
        if (SelectedSakin == null)
        {
            await DialogService.ShowWarningAsync("Lütfen bir sakin seçin.");
            return;
        }
        RequestCikisDialog?.Invoke(this, SelectedSakin);
    }

    public void OnSave(Sakin? existing, int daireId, string ad, string soyad, string? tc, string? tel, string? email, SiteYonetim.Core.Enums.SakinTipi tip, DateTime giris)
    {
        using var db = new AppDbContext();
        if (existing == null)
        {
            db.Sakinler.Add(new Sakin { DaireId = daireId, Ad = ad, Soyad = soyad, TcKimlik = tc, Telefon = tel, Email = email, Tip = tip, GirisTarihi = giris, Aktif = true });
        }
        else
        {
            var e = db.Sakinler.Find(existing.Id);
            if (e != null)
            {
                e.DaireId = daireId; e.Ad = ad; e.Soyad = soyad; e.TcKimlik = tc;
                e.Telefon = tel; e.Email = email; e.Tip = tip; e.GirisTarihi = giris;
            }
        }
        db.SaveChanges();
        LoadData();
    }

    public void OnCikis(int sakinId, DateTime cikisTarihi)
    {
        using var db = new AppDbContext();
        var s = db.Sakinler.Find(sakinId);
        if (s != null) { s.Aktif = false; s.CikisTarihi = cikisTarihi; db.SaveChanges(); }
        LoadData();
    }
}
