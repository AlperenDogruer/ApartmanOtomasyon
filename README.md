# Site Yonetim Otomasyon

Yerel (offline), tek kullanıcılı Site Yönetim Otomasyon masaüstü uygulaması. **macOS, Windows ve Linux** üzerinde çalışır.

## Teknolojiler

- C# .NET 8
- **Avalonia UI** (cross-platform) — macOS, Windows, Linux
- Windows Forms (eski Windows-only sürüm, `SiteYonetim.UI` projesi)
- Entity Framework Core + SQLite
- CommunityToolkit.Mvvm
- PdfSharpCore (PDF export)
- EPPlus (Excel export)

## Son Kullanıcı Kurulumu

Hiçbir ek yazılım gerekmez. Installer her şeyi içerir.

- **Windows**: `SiteYonetimSetup_Windows_v1.0.0.exe` dosyasını çalıştır → İleri → Kur → Bitti
- **macOS (Apple Silicon)**: `SiteYonetim_macOS_AppleSilicon_v1.0.0.zip` aç → uygulamayı Applications'a sürükle
- **macOS (Intel)**: `SiteYonetim_macOS_Intel_v1.0.0.zip` aç → uygulamayı Applications'a sürükle

## Geliştirici Kurulumu

### Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- macOS 11+ (Intel veya Apple Silicon), Windows 10/11 veya modern Linux

### Kaynak Koddan Çalıştırma

```bash
# Projeyi derle
dotnet build

# Avalonia UI uygulamasını çalıştır
dotnet run --project SiteYonetim.UI.Avalonia
```

### Dağıtım Paketi Oluşturma

**Windows:**
```bash
# 1. Self-contained exe oluştur
publish-windows.bat

# 2. Inno Setup ile installer derle (https://jrsoftware.org/isinfo.php)
#    setup.iss dosyasını Inno Setup ile aç ve derle
#    Çıktı: installer_output/SiteYonetimSetup_Windows_v1.0.0.exe
```

**macOS (Mac üzerinde çalıştır):**
```bash
chmod +x publish-mac.sh
./publish-mac.sh
# Çıktı: installer_output/SiteYonetim_macOS_AppleSilicon_v1.0.0.zip
# Çıktı: installer_output/SiteYonetim_macOS_Intel_v1.0.0.zip
```

### Windows (eski WinForms sürümü)

```bash
dotnet run --project SiteYonetim.UI
```

## Proje Yapısı

```
SiteYonetim.sln
├── SiteYonetim.Core/          # Paylaşılan DTO, Enum, Helper (cross-platform)
├── SiteYonetim.Data/          # EF Core + SQLite (cross-platform)
├── SiteYonetim.Business/      # Servisler / iş mantığı (cross-platform)
├── SiteYonetim.UI.Avalonia/   # Avalonia UI — macOS/Windows/Linux
│   ├── ViewModels/            # MVVM ViewModel'ler
│   └── Views/                 # Avalonia XAML + code-behind
└── SiteYonetim.UI/            # WinForms UI — yalnızca Windows
    ├── Controls/              # UserControl'ler
    └── Dialogs/               # Dialog formları
```

## Özellikler

- **Dashboard**: Aylık özet kartları, borçlu daire listesi
- **Daire & Sakin Yönetimi**: Blok/daire ağacı, sakin kayıt ve takibi
- **Aidat İşlemleri**: Toplu tahakkuk, tahsilat girişi, gecikme tazminatı
- **Gelir & Gider**: Gelir/gider kaydı, kasa özeti, kategori bazlı analiz
- **Raporlar**: Borçlular listesi, daire ekstresi, yıllık mizan, gelir-gider raporu
- **PDF ve Excel Export**: Tüm raporlar PDF ve Excel olarak indirilebilir
- **Ayarlar**: Site bilgileri, aidat tipleri, gecikme faiz oranı, veritabanı yedekleme

## Veritabanı Konumu

SQLite veritabanı otomatik olarak platform bazlı olarak aşağıdaki konumda oluşturulur:

- **macOS**: `~/Library/Application Support/SiteYonetim/siteYonetim.db`
- **Windows**: `%APPDATA%\SiteYonetim\siteYonetim.db`
- **Linux**: `~/.config/SiteYonetim/siteYonetim.db`

Her iki UI sürümü de aynı veritabanını paylaşır — Windows VM'de WinForms sürümünde girilen veriler Mac'teki Avalonia sürümünde görünür.

## İlk Çalıştırma

Uygulama ilk açıldığında kurulum sihirbazı görünür:
1. Site adı girişi
2. Yönetici adı girişi
3. Örnek veriler otomatik oluşturulur (2 blok, 6 daire, 1 aidat tipi)
