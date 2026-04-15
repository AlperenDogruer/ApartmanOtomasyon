# Site Yonetim Otomasyon

Windows uzerinde tamamen yerel (offline) calisan, tek kullanicili Site Yonetim Otomasyon masaustu uygulamasi.

## Teknolojiler

- C# .NET 8
- Windows Forms
- Entity Framework Core + SQLite
- PdfSharpCore (PDF export)
- EPPlus (Excel export)

## Gereksinimler

- .NET 8 SDK (https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11

## Kurulum ve Calistirma

```bash
# Projeyi derle
dotnet build

# Uygulamayi calistir
dotnet run --project SiteYonetim.UI
```

## Proje Yapisi

```
SiteYonetim.sln
├── SiteYonetim.Core/        # Paylasilan DTO, Enum, Helper
├── SiteYonetim.Data/        # EF Core + SQLite (Entity, DbContext)
├── SiteYonetim.Business/    # Servisler / is mantigi
└── SiteYonetim.UI/          # Windows Forms (Startup projesi)
    ├── Controls/             # UserControl'ler (Dashboard, Daire, Sakin, Aidat, GelirGider, Raporlar, Ayarlar)
    └── Dialogs/              # Dialog formlari
```

## Ozellikler

- **Dashboard**: Aylik ozet kartlari, borclu daire listesi
- **Daire & Sakin Yonetimi**: Blok/daire agaci, sakin kayit ve takibi
- **Aidat Islemleri**: Toplu tahakkuk, tahsilat girisi, gecikme tazminati
- **Gelir & Gider**: Gelir/gider kaydi, kasa ozeti, kategori bazli analiz
- **Raporlar**: Borclular listesi, daire ekstresi, yillik mizan, gelir-gider raporu
- **PDF ve Excel Export**: Tum raporlar PDF ve Excel olarak indirilebilir
- **Ayarlar**: Site bilgileri, aidat tipleri, gecikme faiz orani, veritabani yedekleme

## Veritabani

SQLite veritabani `%APPDATA%\SiteYonetim\siteYonetim.db` konumunda otomatik olusturulur.

## Ilk Calistirma

Uygulama ilk acildiginda kurulum sihirbazi gorunur:
1. Site adi girisi
2. Yonetici adi girisi
3. Ornek veriler otomatik olusturulur (2 blok, 6 daire, 1 aidat tipi)
