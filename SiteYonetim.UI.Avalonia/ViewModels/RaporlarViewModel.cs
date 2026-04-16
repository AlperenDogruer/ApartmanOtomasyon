using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SiteYonetim.Business.Services;
using SiteYonetim.Core.Helpers;
using SiteYonetim.Data;
using SiteYonetim.UI.Avalonia.Services;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SiteYonetim.UI.Avalonia.ViewModels;

public partial class RaporlarViewModel : ViewModelBase
{
    public ObservableCollection<string> RaporList { get; } = new()
    {
        "Borçlular Listesi", "Daire Ekstresi", "Yıllık Mizan", "Gelir-Gider Raporu"
    };
    public ObservableCollection<string> AylarList { get; } = new(AidatViewModel.Aylar);
    public ObservableCollection<DaireItem> Daireler { get; } = new();

    [ObservableProperty] private int _selectedRaporIndex;
    [ObservableProperty] private int _yil = DateTime.Now.Year;
    [ObservableProperty] private int _ayIndex = DateTime.Now.Month - 1;
    [ObservableProperty] private DaireItem? _selectedDaire;
    [ObservableProperty] private DateTime _bas = new(DateTime.Now.Year, 1, 1);
    [ObservableProperty] private DateTime _bit = DateTime.Now;

    [ObservableProperty] private bool _borcluParamsVisible = true;
    [ObservableProperty] private bool _ekstreParamsVisible;
    [ObservableProperty] private bool _mizanParamsVisible;
    [ObservableProperty] private bool _gelirGiderParamsVisible;

    [ObservableProperty] private DataTable? _previewData;

    private string[]? _currentHeaders;
    private string _currentReportName = "Rapor";

    public event EventHandler<string>? RequestSaveFile; // "pdf" or "excel"

    public RaporlarViewModel()
    {
        LoadDaireler();
    }

    partial void OnSelectedRaporIndexChanged(int value)
    {
        BorcluParamsVisible = value == 0;
        EkstreParamsVisible = value == 1;
        MizanParamsVisible = value == 2;
        GelirGiderParamsVisible = value == 3;
    }

    private void LoadDaireler()
    {
        try
        {
            using var db = new AppDbContext();
            foreach (var d in db.Daireler.Include(x => x.Blok)
                         .OrderBy(x => x.Blok.Ad).ThenBy(x => x.DaireNo).ToList())
                Daireler.Add(new DaireItem { Id = d.Id, Display = $"{d.Blok.Ad} - Daire {d.DaireNo}" });
            if (Daireler.Count > 0) SelectedDaire = Daireler[0];
        }
        catch { }
    }

    [RelayCommand]
    private async Task OnizleAsync()
    {
        try
        {
            var svc = new RaporService();
            switch (SelectedRaporIndex)
            {
                case 0:
                    PreviewData = svc.BorcluListesiRaporu(Yil, AyIndex + 1);
                    _currentHeaders = new[] { "Blok", "Daire No", "Sakin", "Borç Tutarı", "Gecikme Tazminatı", "Durum" };
                    _currentReportName = "Borçlular Listesi";
                    break;
                case 1:
                    if (SelectedDaire == null) { await DialogService.ShowWarningAsync("Lütfen bir daire seçin."); return; }
                    PreviewData = svc.DaireEkstreRaporu(SelectedDaire.Id, Bas, Bit);
                    _currentHeaders = new[] { "Dönem", "Aidat Tipi", "Tahakkuk", "Ödenen", "Kalan", "Gecikme Taz." };
                    _currentReportName = "Daire Ekstresi";
                    break;
                case 2:
                    PreviewData = svc.YillikMizanRaporu(Yil);
                    _currentHeaders = new[] { "Ay", "Gelir", "Gider", "Net" };
                    _currentReportName = "Yıllık Mizan";
                    break;
                case 3:
                    PreviewData = svc.GelirGiderRaporu(Bas, Bit);
                    _currentHeaders = new[] { "Tür", "Tarih", "Açıklama", "Kategori", "Tutar" };
                    _currentReportName = "Gelir-Gider Raporu";
                    break;
            }
        }
        catch (Exception ex) { await DialogService.ShowErrorAsync($"Rapor yüklenirken hata: {ex.Message}"); }
    }

    [RelayCommand] private void Pdf() => RequestSaveFile?.Invoke(this, "pdf");
    [RelayCommand] private void Excel() => RequestSaveFile?.Invoke(this, "excel");

    public async Task SaveAsync(string type, string path)
    {
        if (PreviewData == null || _currentHeaders == null)
        {
            await DialogService.ShowWarningAsync("Önce raporu önizleyin.");
            return;
        }
        try
        {
            if (type == "pdf")
                ExportHelper.ExportToPdf(PreviewData, _currentHeaders, _currentReportName, path);
            else
                ExportHelper.ExportToExcel(PreviewData, _currentHeaders, _currentReportName, path);
            await DialogService.ShowInfoAsync($"Dosya kaydedildi:\n{path}");
        }
        catch (Exception ex) { await DialogService.ShowErrorAsync($"Kaydedilirken hata: {ex.Message}"); }
    }
}
