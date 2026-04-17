@echo off
echo ============================================
echo  Site Yonetim Otomasyon - Windows Build
echo ============================================
echo.

echo [1/2] Avalonia uygulamasi derleniyor (Windows x64, self-contained)...
dotnet publish SiteYonetim.UI.Avalonia/SiteYonetim.UI.Avalonia.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:PublishTrimmed=false ^
    -o publish/windows

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [HATA] Build basarisiz oldu!
    pause
    exit /b 1
)

echo.
echo [2/2] Build tamamlandi!
echo.
echo  Cikti: publish\windows\SiteYonetim.UI.Avalonia.exe
echo.
echo  Installer olusturmak icin:
echo    Inno Setup ile setup.iss dosyasini derleyin.
echo    Cikti: installer_output\SiteYonetimSetup_Windows_v1.0.0.exe
echo.
pause
